BasicallyFramework

A framework made for physics-based multiplayer games. Without needing to do the networking.
While the framework contains common features, the framework might need to be modified to accomodate to your game's needs.

The framework handles sending data a little differently than other frameworks (Mirror, UNet)
Basically does not use RPCs in anyway, it is purely message based.
This is a style that allows you to know what exactly is going on. This is perfect for times where performance is key. (Also it is the style I use, I really like to know what exactly goes on)
Unless you are only going to use the Entity Parameters. (Should only be used to send entity data)

- FEATURES AND OPTIONS
Basically adds a tab on top of the Unity Editor (or MenuItem).
The tab allows you to control which features are compiled. Such as client code, server code, entities, etc..
