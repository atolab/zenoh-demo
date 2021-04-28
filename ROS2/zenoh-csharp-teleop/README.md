# A zenoh C# teleop application for ROS2

## **Requirements**

 * .NET 5.0 (required by `CSCDR` - `zenoh-csharp` supports `netstandard2.0` framework minimum)
 * A [zenoh router](http://zenoh.io/docs/getting-started/quick-test/)
 * The [zenoh/DDS bridge](https://github.com/eclipse-zenoh/zenoh-plugin-dds#trying-it-out)
 * [zenoh-csharp](https://github.com/eclipse-zenoh/zenoh-csharp) 
   (automatically retrieved by .NET from [NuGet](https://www.nuget.org/packages/Zenoh))
 * [CSCDR](https://github.com/atolab/CSCDR)
   (automatically retrieved by .NET from [NuGet](https://www.nuget.org/packages/CSCDR))
 * ROS2 [turtlesim](http://wiki.ros.org/turtlesim) (or any other robot able to receive Twist messages...)

-----
## **Usage**

A simple teleop client publishing Twists and to subscribing to Logs via zenoh, bridged to ROS2.

 1. Start the turtlesim:
      ```bash
      ros2 run turtlesim turtlesim_node
      ```
 2. Start the zenoh router:
      ```bash
      zenohd
      ```
 3. Start the zenoh/DDS bridge:
      ```bash
      zenoh-bridge-dds
      ```
 4. Start Ros2Teleop
      ```bash
      dotnet run -p Ros2Teleop.csproj
      ```
 5. Use the arrows keys to drive the robot

See more use cases in [this blog](https://zenoh.io/blog/2021-04-28-ros2-integration/).

**Notes**:

See all options accepted by Ros2Teleop with:
  ```bash
  dotnet run -p Ros2Teleop.csproj -- -h
  ```

By default Ros2Teleop publishes Twist messages on topic `/rt/turtle1/cmd_vel` (for turtlesim).
For other robot, change the topic using the `-cmd_vel` option:
  ```bash
  dotnet run -p Ros2Teleop.csproj -- -cmd_vel /rt/my_robot/cmd_vel
  ```

Both zenoh router and Teleop can be deployed in different networks than the robot. Only the zenoh/DDS bridge has to run in the same network than the robot (for DDS communication via UDP multicast).  
For instance, you can:
 * deploy the zenoh router in a cloud on a public IP with port 7447 open
 * configure the zenoh bridge to connect this remote zenoh router:
     ```bash
     zenoh-bridge-dds -m client -e tcp/<cloud_ip>:7447
     ```
 * configure Ros2Teleop to connect this remote zenoh router:
    ```bash
    dotnet run -p Ros2Teleop.csproj -- -m client -e tcp/<cloud_ip>:7447
    ```
