using Grpc.Core;
using Grpc.Core.Utils;
using Polly;

namespace grpc_middleware_client
{
    public class BasicCallInvoker : CallInvoker
    {
        private readonly Channel _Channel;

        private readonly Policy _UnaryCallPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="Grpc.Core.DefaultCallInvoker"/> class.
        /// </summary>
        /// <param name="channel">Channel to use.</param>
        public BasicCallInvoker(Channel channel, Policy unaryCallPolicy)
        {
            _Channel = GrpcPreconditions.CheckNotNull(channel);
            _UnaryCallPolicy = unaryCallPolicy;
        }

        /// <summary>
        /// Invokes a simple remote call in a blocking fashion.
        /// </summary>
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            var call = CreateCall(method, host, options);

            TResponse response = null;
            _UnaryCallPolicy.Execute(() => {
                response = Calls.BlockingUnaryCall(call, request);
                
            });
            return response;
        }

        /// <summary>
        /// Invokes a simple remote call asynchronously.
        /// </summary>
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            var call = CreateCall(method, host, options);
            var response = Calls.AsyncUnaryCall(call, request);

            return response;
        }

        /// <summary>
        /// Invokes a server streaming call asynchronously.
        /// In server streaming scenario, client sends on request and server responds with a stream of responses.
        /// </summary>
        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            var call = CreateCall(method, host, options);
            var response = Calls.AsyncServerStreamingCall(call, request);

            return response;
        }

        /// <summary>
        /// Invokes a client streaming call asynchronously.
        /// In client streaming scenario, client sends a stream of requests and server responds with a single response.
        /// </summary>
        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            var call = CreateCall(method, host, options);
            var response = Calls.AsyncClientStreamingCall(call);

            return response;
        }

        /// <summary>
        /// Invokes a duplex streaming call asynchronously.
        /// In duplex streaming scenario, client sends a stream of requests and server responds with a stream of responses.
        /// The response stream is completely independent and both side can be sending messages at the same time.
        /// </summary>
        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            var call = CreateCall(method, host, options);
            var response = Calls.AsyncDuplexStreamingCall(call);

            return response;
        }

        /// <summary>Creates call invocation details for given method.</summary>
        protected virtual CallInvocationDetails<TRequest, TResponse> CreateCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
                where TRequest : class
                where TResponse : class
        {
            return new CallInvocationDetails<TRequest, TResponse>(_Channel, method, host, options);
        }
    }
}
