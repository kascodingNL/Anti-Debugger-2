var CONFIG = require('./config.json');

const WebSocket = require('ws');
var crypto = require('crypto');
var pbkdf2 = require('pbkdf2')

const ws = new WebSocket.Server({ port: CONFIG.port });
var connectedArray = [];

console.log("Starting server with SecureHash " + CONFIG.SecureKey + " and " + CONFIG.SecureRounds + " rounds");

var key = CONFIG.SecureKey;
var Salt = crypto.randomBytes(CONFIG.SaltLenght);
var Rounds = CONFIG.SecureRounds;
var derivedKey = pbkdf2.pbkdf2Sync(key, Salt, Rounds, 32, 'sha512')


console.log('Started on port 8180! Hashed securekey is ' + derivedKey);

ws.on('connection', function connection(ws) {
	ws.on('message', function incoming(message) {

		const filtered = message.toString().replace(/[^\x20-\x7E]/g, '');
		
		if(CONFIG.debug)
		{
			console.log('received: %s', filtered);
		}
		var splitted = filtered.split(' ');
	
		if (splitted[0] == "handshake" && splitted[1] == "sending" && splitted[2] == "with" && splitted[3] == "id") {
			var secureID = crypto.randomBytes(256 / 9).toString('hex')
			connectedArray.push(secureID);
			ws.send("HandShake with id " + splitted[4] + " allowed SecID: " + secureID);
	
			if (splitted[1] == "handshake" && splitted[0] == "disconnect") {
				const index = connectedArray.indexOf(filtered);
				if (index > -1) {
					connectedArray.splice(index, 1);
				}
			}
	
			if (splitted[0] == "debuggertrue") {
				if (connectedArray.includes(splitted[1])) {
					ws.send("debugquit");
					connectedArray.splice(connectedArray.indexOf(splitted[1]));
				} else {
					ws.send("insecure response, handshake first");
				}
			}
			else {
				ws.send("continue");
			}
		}
	});
});