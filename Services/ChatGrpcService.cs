using System;
using System.Globalization;
using Grpc.Core;
using MongoDB.Bson;
using MongoDB.Driver;
using fuszerkomat_api.Data;
using fuszerkomat_api.Data.Models.Chat;
using fuszerkomat_api.Grpc;
using Microsoft.Extensions.Logging;

namespace fuszerkomat_api.Services
{
    public class ChatGrpcService : Chat.ChatBase
    {
        private readonly ChatCollections _chat;
        private readonly ILogger<ChatGrpcService> _logger;

        public ChatGrpcService(ChatCollections chat, ILogger<ChatGrpcService> logger)
        {
            _chat = chat;
            _logger = logger;
        }

        public override async Task<OpenOrGetConversationResponse> OpenOrGetConversation(
            OpenOrGetConversationRequest request, ServerCallContext context)
        {
            if (request.TaskId <= 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "task_id must be > 0"));
            if (string.IsNullOrWhiteSpace(request.OwnerUserId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "owner_user_id is required"));
            if (string.IsNullOrWhiteSpace(request.CompanyUserId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "company_user_id is required"));

            var ct = context.CancellationToken;

            var filter = Builders<Conversation>.Filter.And(
                Builders<Conversation>.Filter.Eq(x => x.TaskId, request.TaskId),
                Builders<Conversation>.Filter.Eq(x => x.OwnerUserId, request.OwnerUserId),
                Builders<Conversation>.Filter.Eq(x => x.CompanyUserId, request.CompanyUserId)
            );

            var existing = await _chat.Conversations.Find(filter).FirstOrDefaultAsync(ct);
            if (existing != null)
            {
                return new OpenOrGetConversationResponse
                {
                    ConversationId = existing.Id.ToString(),
                    Created = false
                };
            }

            var now = DateTime.UtcNow;
            var convo = new Conversation
            {
                Id = ObjectId.GenerateNewId(),
                TaskId = request.TaskId,
                OwnerUserId = request.OwnerUserId,
                CompanyUserId = request.CompanyUserId,
                CreatedAt = now,
                LastMessageAt = now
            };

            using var session = await _chat.Conversations.Database.Client.StartSessionAsync(cancellationToken: ct);
            session.StartTransaction();
            try
            {
                await _chat.Conversations.InsertOneAsync(session, convo, cancellationToken: ct);

                if (!string.IsNullOrWhiteSpace(request.InitialMessage))
                {
                    var firstMsg = new fuszerkomat_api.Data.Models.Chat.Message
                    {
                        Id = ObjectId.GenerateNewId(),
                        ConversationId = convo.Id,
                        SenderId = request.OwnerUserId,
                        Text = request.InitialMessage,
                        CreatedAt = now
                    };
                    await _chat.Messages.InsertOneAsync(session, firstMsg, cancellationToken: ct);

                    var update = Builders<Conversation>.Update.Set(x => x.LastMessageAt, now);
                    await _chat.Conversations.UpdateOneAsync(session,
                        Builders<Conversation>.Filter.Eq(x => x.Id, convo.Id), update, cancellationToken: ct);
                }

                await session.CommitTransactionAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create conversation");
                await session.AbortTransactionAsync(ct);
                throw new RpcException(new Status(StatusCode.Internal, "Failed to create conversation"));
            }

            return new OpenOrGetConversationResponse
            {
                ConversationId = convo.Id.ToString(),
                Created = true
            };
        }

        public override async Task<SendMessageResponse> SendMessage(
            SendMessageRequest request, ServerCallContext context)
        {
            if (string.IsNullOrWhiteSpace(request.ConversationId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "conversation_id is required"));
            if (!ObjectId.TryParse(request.ConversationId, out var convId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "conversation_id must be a valid ObjectId"));
            if (string.IsNullOrWhiteSpace(request.SenderId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "sender_id is required"));
            if (string.IsNullOrWhiteSpace(request.Text))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "text is required"));

            var ct = context.CancellationToken;

            var convo = await _chat.Conversations.Find(c => c.Id == convId).FirstOrDefaultAsync(ct);
            if (convo == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Conversation not found"));

            var now = DateTime.UtcNow;
            var msg = new fuszerkomat_api.Data.Models.Chat.Message
            {
                Id = ObjectId.GenerateNewId(),
                ConversationId = convId,
                SenderId = request.SenderId,
                Text = request.Text,
                CreatedAt = now
            };

            using var session = await _chat.Conversations.Database.Client.StartSessionAsync(cancellationToken: ct);
            session.StartTransaction();
            try
            {
                await _chat.Messages.InsertOneAsync(session, msg, cancellationToken: ct);

                var update = Builders<Conversation>.Update.Set(x => x.LastMessageAt, now);
                await _chat.Conversations.UpdateOneAsync(session,
                    Builders<Conversation>.Filter.Eq(x => x.Id, convId), update, cancellationToken: ct);

                await session.CommitTransactionAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message");
                await session.AbortTransactionAsync(ct);
                throw new RpcException(new Status(StatusCode.Internal, "Failed to send message"));
            }

            return new SendMessageResponse
            {
                MessageId = msg.Id.ToString(),
                CreatedAt = now.ToString("o", CultureInfo.InvariantCulture)
            };
        }

        public override async Task<ListMessagesResponse> ListMessages(
            ListMessagesRequest request, ServerCallContext context)
        {
            if (string.IsNullOrWhiteSpace(request.ConversationId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "conversation_id is required"));
            if (!ObjectId.TryParse(request.ConversationId, out var convId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "conversation_id must be a valid ObjectId"));

            var ct = context.CancellationToken;
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 200);

            var exists = await _chat.Conversations.Find(c => c.Id == convId).AnyAsync(ct);
            if (!exists)
                throw new RpcException(new Status(StatusCode.NotFound, "Conversation not found"));

            var filter = Builders<fuszerkomat_api.Data.Models.Chat.Message>.Filter.Eq(m => m.ConversationId, convId);

            var total = (int)await _chat.Messages.CountDocumentsAsync(filter, cancellationToken: ct);

            var items = await _chat.Messages.Find(filter)
                .Sort(Builders<fuszerkomat_api.Data.Models.Chat.Message>.Sort.Ascending(m => m.CreatedAt))
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(ct);

            var resp = new ListMessagesResponse
            {
                Page = page,
                PageSize = pageSize,
                Total = total
            };
            resp.Items.AddRange(items.Select(m => new fuszerkomat_api.Grpc.Message
            {
                MessageId = m.Id.ToString(),
                SenderId = m.SenderId,
                Text = m.Text,
                CreatedAt = m.CreatedAt.ToString("o", CultureInfo.InvariantCulture)
            }));

            return resp;
        }

        public override async Task SubscribeMessages(
            SubscribeMessagesRequest request,
            IServerStreamWriter<fuszerkomat_api.Grpc.Message> responseStream,
            ServerCallContext context)
        {
            if (string.IsNullOrWhiteSpace(request.ConversationId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "conversation_id is required"));
            if (!ObjectId.TryParse(request.ConversationId, out var convId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "conversation_id must be a valid ObjectId"));

            var ct = context.CancellationToken;

            var exists = await _chat.Conversations.Find(c => c.Id == convId).AnyAsync(ct);
            if (!exists)
                throw new RpcException(new Status(StatusCode.NotFound, "Conversation not found"));

            DateTime? since = null;
            if (!string.IsNullOrWhiteSpace(request.Since) &&
                DateTime.TryParse(request.Since, null, DateTimeStyles.RoundtripKind, out var parsed))
            {
                since = parsed.ToUniversalTime();
            }

            if (since.HasValue)
            {
                var backlogFilter = Builders<fuszerkomat_api.Data.Models.Chat.Message>.Filter.And(
                    Builders<fuszerkomat_api.Data.Models.Chat.Message>.Filter.Eq(m => m.ConversationId, convId),
                    Builders<fuszerkomat_api.Data.Models.Chat.Message>.Filter.Gt(m => m.CreatedAt, since.Value)
                );

                var backlog = await _chat.Messages.Find(backlogFilter)
                    .Sort(Builders<fuszerkomat_api.Data.Models.Chat.Message>.Sort.Ascending(m => m.CreatedAt))
                    .Limit(200)
                    .ToListAsync(ct);

                foreach (var m in backlog)
                {
                    await responseStream.WriteAsync(new fuszerkomat_api.Grpc.Message
                    {
                        MessageId = m.Id.ToString(),
                        SenderId = m.SenderId,
                        Text = m.Text,
                        CreatedAt = m.CreatedAt.ToString("o", CultureInfo.InvariantCulture)
                    });
                }
            }


            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<fuszerkomat_api.Data.Models.Chat.Message>>()
                .Match(cs => cs.OperationType == ChangeStreamOperationType.Insert &&
                             cs.FullDocument.ConversationId == convId);

            var options = new ChangeStreamOptions { FullDocument = ChangeStreamFullDocumentOption.Default };

            using var cursor = await _chat.Messages.WatchAsync(pipeline, options, ct);


            await foreach (var change in cursor.ToAsyncEnumerable())
            {
                if (ct.IsCancellationRequested)
                    break;

                var doc = change.FullDocument;
                if (doc == null) continue;

                await responseStream.WriteAsync(new fuszerkomat_api.Grpc.Message
                {
                    MessageId = doc.Id.ToString(),
                    SenderId = doc.SenderId,
                    Text = doc.Text,
                    CreatedAt = doc.CreatedAt.ToString("o", CultureInfo.InvariantCulture)
                });
            }

        }
    }
}
