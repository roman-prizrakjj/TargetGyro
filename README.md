This script is written in ```C#``` and is used in the game Space Engineers
There are two separate functions in the script

the first function: 
```CS 
Debag(string text, object prog = null, bool empty = true)
```
Outputs the data transmitted to it to the service panel that you will assign to it.

The second function:
```cs
aiming(Vector3D target, string argument)
```
Accepts vector3D coordinates in the world and uses gyroscopes to guide the ship to a given point.

