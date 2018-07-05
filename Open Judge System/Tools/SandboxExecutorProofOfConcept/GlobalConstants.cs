namespace SandboxExecutorProofOfConcept
{
    internal class GlobalConstants
    {
        public const string SampleJavaScriptCode = @"
var EOL = require('os').EOL;

DataView = undefined;
DTRACE_NET_SERVER_CONNECTION = undefined;
// DTRACE_NET_STREAM_END = undefined;
DTRACE_NET_SOCKET_READ = undefined;
DTRACE_NET_SOCKET_WRITE = undefined;
DTRACE_HTTP_SERVER_REQUEST = undefined;
DTRACE_HTTP_SERVER_RESPONSE = undefined;
DTRACE_HTTP_CLIENT_REQUEST = undefined;
DTRACE_HTTP_CLIENT_RESPONSE = undefined;
COUNTER_NET_SERVER_CONNECTION = undefined;
COUNTER_NET_SERVER_CONNECTION_CLOSE = undefined;
COUNTER_HTTP_SERVER_REQUEST = undefined;
COUNTER_HTTP_SERVER_RESPONSE = undefined;
COUNTER_HTTP_CLIENT_REQUEST = undefined;
COUNTER_HTTP_CLIENT_RESPONSE = undefined;
global = undefined;
process.argv = undefined;
process.versions = undefined;
process.env = { NODE_DEBUG: false };
process.addListener = undefined;
process.EventEmitter = undefined;
process.mainModule = undefined;
process.removeListener = undefined;
process.config = undefined;
process.on = undefined;
process.openStdin = undefined;
process.chdir = undefined;
process.cwd = undefined;
process.umask = undefined;
GLOBAL = undefined;
root = undefined;
setTimeout = undefined;
setInterval = undefined;
clearTimeout = undefined;
clearInterval = undefined;
setImmediate = undefined;
clearImmediate = undefined;
module = undefined;
require = undefined;
msg = undefined;

delete DataView;
delete DTRACE_NET_SERVER_CONNECTION;
// delete DTRACE_NET_STREAM_END;
delete DTRACE_NET_SOCKET_READ;
delete DTRACE_NET_SOCKET_WRITE;
delete DTRACE_HTTP_SERVER_REQUEST;
delete DTRACE_HTTP_SERVER_RESPONSE;
delete DTRACE_HTTP_CLIENT_REQUEST;
delete DTRACE_HTTP_CLIENT_RESPONSE;
delete COUNTER_NET_SERVER_CONNECTION;
delete COUNTER_NET_SERVER_CONNECTION_CLOSE;
delete COUNTER_HTTP_SERVER_REQUEST;
delete COUNTER_HTTP_SERVER_RESPONSE;
delete COUNTER_HTTP_CLIENT_REQUEST;
delete COUNTER_HTTP_CLIENT_RESPONSE;
delete global;
delete process.argv;
delete process.versions;
delete GLOBAL;
delete root;
delete setTimeout;
delete setInterval;
delete clearTimeout;
delete clearInterval;
delete setImmediate;
delete clearImmediate;
delete module;
delete require;
delete msg;

var content = '';
process.stdin.resume();
process.stdin.on('data', function(buf) { content += buf.toString(); });
process.stdin.on('end', function() {
    var inputData = content.trim().split(EOL);
    console.log(code.f(inputData));
});

var code = {
    f:function solve(content) {
        // !!! User code starts here
        var answer = '';
        for(var i = 0; i < content.length; i++) {
            answer += content[i];
        }
        return answer + 'new';
        // !!! User code ends here
    }
};

function x()
{
    console.log(this);
}
x();";
    }
}
