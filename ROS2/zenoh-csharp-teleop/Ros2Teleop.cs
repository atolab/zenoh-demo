//
// Copyright (c) 2021 ADLINK Technology Inc.
//
// This program and the accompanying materials are made available under the
// terms of the Eclipse Public License 2.0 which is available at
// http://www.eclipse.org/legal/epl-2.0, or the Apache License, Version 2.0
// which is available at https://www.apache.org/licenses/LICENSE-2.0.
//
// SPDX-License-Identifier: EPL-2.0 OR Apache-2.0
//
// Contributors:
//   ADLINK zenoh team, <zenoh@adlink-labs.tech>
//
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using Zenoh;
using CSCDR;
using PowerArgs;

class Teleop
{
    private Zenoh.Net.Session _session;
    private Zenoh.Net.ResKey _cmdKey;
    private double _linearScale;
    private double _angularScale;

    private Teleop(Dictionary<string, string> zenohConf, String cmdTopic, String outTopic, double linearScale, double angularScale)
    {
        // initiate logging
        Zenoh.Zenoh.InitLogger();

        Console.WriteLine("Openning session...");
        _session = Zenoh.Net.Session.Open(zenohConf);

        Console.WriteLine("Subscribing on {0}", outTopic);
        _session.DeclareSubscriber(
            Zenoh.Net.ResKey.RName(outTopic),
            new Zenoh.Net.SubInfo(),
            OutCallback);

        Console.WriteLine("Publishing on {0}", cmdTopic);
        _cmdKey = Zenoh.Net.ResKey.RName(cmdTopic);
        _linearScale = linearScale;
        _angularScale = angularScale;
    }

    private void PubTwist(double linear, double angular)
    {
        // type to write: geometry_msgs/msg/Twist
        CDRWriter cdrWriter = new CDRWriter();
        // Vector3 linear => float64 x + float64 y + float64 z
        cdrWriter.WriteDouble(linear * _linearScale);
        cdrWriter.WriteDouble(0.0);
        cdrWriter.WriteDouble(0.0);
        // Vector3 angular => float64 x + float64 y + float64 z
        cdrWriter.WriteDouble(0.0);
        cdrWriter.WriteDouble(0.0);
        cdrWriter.WriteDouble(angular * _angularScale);
        _session.Write(_cmdKey, cdrWriter.GetBuffer().ToArray());
    }

    private void OutCallback(Zenoh.Net.Sample sample)
    {
        // received type: rcl_interfaces/msg/Log
        CDRReader reader = new CDRReader(sample.Payload);
        // builtin_interfaces/Time stamp => int32 sec + uint32 nanosec
        var stamp_sec = reader.ReadInt32();
        var stamp_nanosec = reader.ReadUInt32();
        // uint8 level
        var level = reader.ReadByte();
        // string name
        var name = reader.ReadString();
        // string msg
        var msg = reader.ReadString();
        // string file
        var file = reader.ReadString();
        // string function
        var function = reader.ReadString();
        // uint32 line
        var line = reader.ReadUInt32();

        Console.WriteLine("[{0}.{1}] [{2}]: {3}", stamp_sec, stamp_nanosec, name, msg);
    }

    private void run()
    {
        Console.WriteLine("Waiting commands with arrow keys or space bar to stop. Press ESC to quit.");
        ConsoleKeyInfo cki;
        do
        {
            cki = Console.ReadKey();
            switch (cki.Key)
            {
                case ConsoleKey.UpArrow:
                    {
                        PubTwist(1.0, 0.0);
                        break;
                    }
                case ConsoleKey.DownArrow:
                    {
                        PubTwist(-1.0, 0.0);
                        break;
                    }
                case ConsoleKey.LeftArrow:
                    {
                        PubTwist(0.0, 1.0);
                        break;
                    }
                case ConsoleKey.RightArrow:
                    {
                        PubTwist(0.0, -1.0);
                        break;
                    }
                case ConsoleKey.Spacebar:
                    {
                        PubTwist(0.0, 0.0);
                        break;
                    }
                default: break;
            }
        } while (cki.Key != ConsoleKey.Escape);
        // Stop robot at exit
        PubTwist(0.0, 0.0);
    }


    static void Main(string[] args)
    {
        try
        {
            // arguments parsing
            var arguments = Args.Parse<ExampleArgs>(args);
            if (arguments == null) return;
            Dictionary<string, string> conf = arguments.GetConf();

            var teleop = new Teleop(conf, arguments.cmdTopic, arguments.outTopic, arguments.linearScale, arguments.angularScale);
            teleop.run();
        }
        catch (ArgException)
        {
            Console.WriteLine(ArgUsage.GenerateUsageFromTemplate<ExampleArgs>());
        }
    }
}


[ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
public class ExampleArgs
{
    [HelpHook, ArgShortcut("h"), ArgDescription("Shows this help")]
    public Boolean help { get; set; }

    [ArgShortcut("m"), ArgDefaultValue("peer"), ArgDescription("The zenoh session mode. Possible values [peer|client].")]
    public string mode { get; set; }

    [ArgShortcut("e"), ArgDescription("Peer locators used to initiate the zenoh session.")]
    public string peer { get; set; }

    [ArgShortcut("l"), ArgDescription("Locators to listen on.")]
    public string listener { get; set; }

    [ArgShortcut("c"), ArgDescription("A configuration file.")]
    public string config { get; set; }

    [ArgShortcut("cmd_vel"), ArgDefaultValue("/rt/turtle1/cmd_vel"), ArgDescription("The 'cmd_vel' ROS2 topic")]
    public string cmdTopic { get; set; }

    [ArgShortcut("rosout"), ArgDefaultValue("/rt/rosout"), ArgDescription("The 'rosout' ROS2 topic")]
    public string outTopic { get; set; }

    [ArgShortcut("a"), ArgDefaultValue(2.0), ArgDescription("The angular scale.")]
    public double angularScale { get; set; }

    [ArgShortcut("x"), ArgDefaultValue(2.0), ArgDescription("The linear scale.")]
    public double linearScale { get; set; }

    public Dictionary<string, string> GetConf()
    {
        var conf = new Dictionary<string, string>();
        conf.Add("mode", this.mode);
        if (this.peer != null)
        {
            conf.Add("peer", this.peer);
        }
        if (this.listener != null)
        {
            conf.Add("listener", this.listener);
        }
        if (this.config != null)
        {
            conf.Add("config", this.config);
        }
        return conf;
    }
}
