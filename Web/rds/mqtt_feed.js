
var mqtt = require('mqtt');
var url = require('url');
var Client = require('node-rest-client').Client;
var client = new Client();
var http = require('http');
var parseString = require('xml2js').parseString;
var async = require("async");

var feed_data = function () {
    client.get("http://localhost:3000/feed", function (feed_provider, response) {
        // console.log(feed_provider);
        // async.each(feed_provider,
        //     function (feed, next) {

        //         console.log(feed);
        //        // next();

        //         var get_url = "http://"+feed_provider[0].user+":"+feed_provider[0].pass+"@"+feed_provider[0].url;
        //         xmlToJson(get_url, function (err, rds_data) {
        //             console.log("dfsdfsdfsdfs");
        //             next();
        //         });
        //     }

        //     , function (err) {

        //     });
        //for (var i = 0; i < feed_provider.length; i++) {
        var get_url = "http://"+feed_provider[0].user+":"+feed_provider[0].pass+"@"+feed_provider[0].url;
        xmlToJson(get_url, function (err, rds_data) {
            if (err) {
                return console.err(err);
            }
            // rds_data.TrafficStatus.TrafficMessage.length
            for (var j = 0; j <rds_data.TrafficStatus.TrafficMessage.length; j++) {
                var tag = "data";
                var locCode = rds_data.TrafficStatus.TrafficMessage[j].Location[0].Segment[0].From[0].locCode;
                var direction = rds_data.TrafficStatus.TrafficMessage[j].Location[0].Segment[0].From[0].direction;
                var quantity = rds_data.TrafficStatus.TrafficMessage[j].Event[0].Quantity;
                var EventCode = rds_data.TrafficStatus.TrafficMessage[j].Event[0].EventCode;
                var all_data = tag + "," + locCode + "," + direction + "," + quantity + "," + EventCode;
                
                // var locCode = rds_data.TrafficStatus.TrafficMessage[j].Location[0].Point[0].locCode;
                // var direction = rds_data.TrafficStatus.TrafficMessage[j].Location[0].Point[0].direction;
                // var quantity = rds_data.TrafficStatus.TrafficMessage[j].Event[0].Quantity;
                // var quanttype = rds_data.TrafficStatus.TrafficMessage[j].Event[0].QuantType;
                // var all_data = tag + "," + locCode + "," + direction + "," + quantity + "," + quanttype;
                
                
                //console.log(all_data);
                client2.publish(feed_provider[0].city, all_data, function () {
                    //client2.end(); // Close the connection when published
                });
            }
        });
        //    }
    });
}

var xmlToJson = function (url, callback) {
    var req = http.get(url, function (res) {
        var xml = '';

        res.on('data', function (chunk) {
            xml += chunk;
        });

        res.on('error', function (e) {
            callback(e, null);
        });

        res.on('timeout', function (e) {
            callback(e, null);
        });

        res.on('end', function () {
            parseString(xml, function (err, result) {
                callback(null, result);
            });
        });
    });
}


// Parse 
var mqtt_url = url.parse(process.env.CLOUDMQTT_URL || 'mqtt://m10.cloudmqtt.com:10277');
var auth = (mqtt_url.auth || ':').split(':');
var url2 = "mqtt://" + mqtt_url.host;

var options = {
    port: mqtt_url.port,
    clientId: 'mqttjs_' + Math.random().toString(16).substr(2, 8),
    username: "ovwnmwny",
    password: "zERBTx4cWMeG",
};

// Create a client connection
var client2 = mqtt.connect(url2, options);

client2.on('connect', function () { // When connected

    // subscribe to a topic
    client2.subscribe('cloudmqtt', function () {
        // when a message arrives, do something with it
        client2.on('message', function (topic, message, packet) {
            console.log("Received '" + message + "' on '" + topic + "'");
        });
    });
    // publish a message to a topic
    // feed_data();
    setInterval(feed_data, 60000);
});






            // http.get(feed_provider[i].url
            // , function (rds_data, responsexx) {
            //         //for rds_data
            //         // for(var j=0;j<rds_data;j++){
            //         //     client.publish(feed_provider[i].name, rds_data[j], function () {
            //         //         console.log("Message is published");
            //         //         client.end(); // Close the connection when published
            //         //     });
            //         // }
            //         console.log("rds_data"); 
            //         console.log(rds_data); 
            // });
