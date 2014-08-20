Cloud NES - Server Logic
===============

####*This project was made in 36 hours at LA Hacks hackathon. This repository is the tcp server-sided logic only.* 

For the iOS app repository, please visit: https://github.com/joelgreen/MegaSegaController
Website is live at http://thecloudnes.com/

Cloud NES is a website that allows you to play many classic NES games with your keyboard, or with our app on any iOS device. We also support multiplayer, so users with our app can sync their app with our web page and start playing together.

###How it works
When a user visits the page, they are given a unique key. They simply enter that unique key into their controller/app, which then syncs up that web page with their phone controller. If a second user enters the same unique key, they are marked as player 2 for multiplayer games. 

###Development
The webpage uses HTML5 web sockets to communicate with the server. My C# server is using the Alchemy Websockets library for the web socket server, and then a native socket implementation for communicating with the controller.

![ScreenShot](http://i.imgur.com/eUfmyjS.png)
![ScreenShot](http://i.imgur.com/oKczOC2.jpg)
![ScreenShot](http://i.imgur.com/etePpTh.jpg)

###Games You Can Play:
![ScreenShot](http://i.imgur.com/r8IFJoc.png)
