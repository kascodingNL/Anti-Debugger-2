const WebSocket = require('ws');
var crypto = require('crypto'); 

const ws = new WebSocket.Server({ port: 8180 });
var connectedArray = [];

console.log('Started on port 8180!');

ws.on('connection', function connection(ws) {
  ws.on('message', function incoming(message) {
	  const filtered = message.toString().replace(/[^\x20-\x7E]/g, '');
	  console.log('received: %s', filtered);
	var splitted = filtered.split(' ');

	if(splitted[0] == "handshake" && splitted[1] == "sending" && splitted[2] == "with" && splitted[3] == "id")
	{
		var secureID = crypto.randomBytes(256 / 9).toString('hex')
		connectedArray.push(secureID);
		ws.send("HandShake with id " + splitted[4] + " allowed SecID: " + secureID);

	if(splitted[1] == "handshake" && splitted[0] == "disconnect")
	{
		const index = connectedArray.indexOf(filtered);
		if(index > -1)
		{
			connectedArray.splice(index, 1);
		}
	}
	
	if(splitted[0] == "debuggertrue")
	{
		if( connectedArray.includes(splitted[1]))
		{
			ws.send("debugquit");
			connectedArray.splice(connectedArray.indexOf(splitted[1]));
		} else
		{
			ws.send("insecure response, handshake first");
		}
	}
	else
	{
		ws.send("continue");
	}
  };
});

ws.on('close', function disconnect(reasonCode, description)
{
	console.log('Disconnect');
});
