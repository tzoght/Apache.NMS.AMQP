﻿/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections;
using Apache.NMS;
using Apache.NMS.Util;
using Apache.NMS.Amqp;

namespace Apache.NMS.ActiveMQ.Test
{
    public class TestMain
    {
        /// <summary>
        /// AMQP Hello World
        /// Using the AMQP protocol, send a message to a topic and retrieve that message again.
        /// </summary>
        /// <param name="uri">AMQP peer network address string. This string selects
        /// the Apache.NMS.AMQP provider and further specifies the TCP address and port of the
        /// peer AMQP entity.</param>
        /// <param name="protocolVersion">Selects AMQP protocol version. Use 'amqp0-10' or 'amqp1.0'.
        /// amqp1.0 is the default version if none is specified.</param>
        /// <param name="topicAddress">The name of the topic through which the message is passed
        /// in the AMQP peer.</param>
        /// </summary>
        public static void AMQP_HelloWorld(string uri, string protocolVersion, string topicAddress)
        {
            // AMQP Hello World
            //
            // Notes:
            //  * Run qpidd broker, activemq broker, or dispatch router on given uri.
            //  * Ensure the nmsprovider-amqp.config file exists 
            //    in the executable folder (build\net4-0\debug).
            //  * Ensure the unmanaged qpid*.dll and boost*.dll files from 
            //      .nant\library\local\org.apache.qpid\Apache.Qpid\<version>\net-4.0\debug
            //      are in project's Output Path (build\net-4.0\debug) so that they may be
            //      loaded by org.apache.qpid.messaging.dll.
            try
            {
                Uri connecturi = new Uri(uri);

                Console.WriteLine("About to connect to " + connecturi);

                Hashtable props = new Hashtable();
                props.Add("protocol", "amqp0-10");
                props.Add("reconnect_timeout", 60);

                IConnectionFactory factory;
                if (false)
                    factory = 
                        new NMSConnectionFactory(connecturi, "Bob", props);
                else
                    factory =
                        new NMSConnectionFactory(connecturi, "Bob", "protocol:amqp0-10", "reconnect_timeout:60");

                using (IConnection connection = factory.CreateConnection())
                using (ISession session = connection.CreateSession())
                {
                    IDestination destination = SessionUtil.GetDestination(session, topicAddress);

                    // Create a consumer and producer
                    using (IMessageConsumer consumer = session.CreateConsumer(destination))
                    using (IMessageProducer producer = session.CreateProducer(destination))
                    {
                        // Start the connection so that messages will be processed.
                        connection.Start();

                        // Create a text message
                        ITextMessage request = session.CreateTextMessage("Hello World!");
                        request.Properties["NMSXGroupID"] = "cheese";
                        request.Properties["myHeader"] = "Cheddar";

                        IMapMessage mmsg = session.CreateMapMessage();
                        mmsg.Body.SetString("key1", "value1");
                        mmsg.Properties["x-amqp-to"] = topicAddress;

                        //Int64[] values = {10,200,3000,40000};
                        bool[] values = {true, false, true, false, true, false};


                        IObjectMessage objMsg = session.CreateObjectMessage(values);
                        objMsg.Properties["number"] = 42;

                        // For dispatch router 0.1 messages require a routing property
                        request.Properties["x-amqp-to"] = topicAddress;

                        // Send the message
                        //producer.Send(request);
                        //producer.Send(mmsg);
                        producer.Send(objMsg);

                        // Consume a message
                        IMessage message = consumer.Receive();
                        if (message == null)
                        {
                            Console.WriteLine("No message received!");
                        }
                        else
                        {
                            if (message is ITextMessage)
                            {
                                TextMessage tm = (TextMessage)message;
                                Console.WriteLine("Received text message text: " + tm.Text);
                                Console.WriteLine("Received text message properties: " + tm.Properties.ToString());
                            }
                            else if (message is IMapMessage)
                            {
                                MapMessage mm = (MapMessage)message;
                                Console.WriteLine("Received map message content: " + mm.Body.ToString());
                                Console.WriteLine("Received map message properties: " + mm.Properties.ToString());
                            }
                            else if (message is IObjectMessage)
                            {
                                ObjectMessage om = (ObjectMessage)message;
                                Object obj = om.Body;
                                Console.WriteLine("Received object message content: " + obj.ToString());
                                Console.WriteLine("Received object message properties: " + om.Properties.ToString());
                                if (obj.GetType().IsArray)
                                {
                                    Array valueArray = (Array)obj;
                                    for (int i =0; i<valueArray.Length; i++)
                                    {
                                        Console.WriteLine("Obj[{0}] = {1}", i, valueArray.GetValue(i));
                                    }
                                }

                            }
                            else
                            {
                            }
                        }
                        connection.Stop();
                    }
                }
            } catch (Exception e) {
                Console.WriteLine("Exception {0}.", e);
            }
        }

        
        public static void Main(string[] args)
        {
            //string uriQpidd = "amqp:localhost:5672";
            string uriActivemq = "amqp:localhost:5672";
            //string uriDispatch = "amqp:localhost:5672";

            //AMQP_HelloWorld(uriQpidd, "amqp0-10", "amq.topic");
            //AMQP_HelloWorld(uriQpidd, "amqp1.0", "amq.topic");

            AMQP_HelloWorld(uriActivemq, "amqp1.0", "amq.topic");

            //AMQP_HelloWorld(uriDispatch, "amqp1.0", "amq.topic");
        }
    }
}