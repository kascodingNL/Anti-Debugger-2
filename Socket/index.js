const WebSocket = require('ws');

const ws = new WebSocket.Server({ port: 8180 });
var connectedArray = {};

console.log('Started on port 8180!');

ws.on('connection', function connection(ws) {
  ws.on('message', function incoming(message) {
	  const filtered = message.toString().replace(/[^\x20-\x7E]/g, '');
	  console.log('received: %s', filtered);
	var splitted = filtered.split(' ');

	if(splitted[1] == "handshake" && splitted[0] == "sending")
	{
		connectedArray.push(filtered);
	}

	if(splitted[1] == "handshake" && splitted[0] == "disconnect")
	{
		const index = connectedArray.indexOf(filtered);
		if(index > -1)
		{
			connectedArray.splice(index, 1);
		}
	}
	
	if(filtered == "debuggertrue")
	{
		ws.send("debugquit");
	}
	else
	{
		ws.send("continue");
	}
  });
});

ws.on('close', function disconnect(reasonCode, description)
{
	console.log('Disconnect');
});
