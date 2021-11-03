using System;
using System.Collections.Generic;
using System.Threading;

namespace Flaskeautomaten
{
    class Program
    {
        //Arrays
        static Bottle[] producerBuffer;
        static Bottle[] sodaBuffer;
        static Bottle[] beerBuffer;

        static void Main(string[] args)
        {

            int maxSize = 15;

            //Makes new shared arrays 
            producerBuffer = new Bottle[maxSize];
            sodaBuffer = new Bottle[maxSize];
            beerBuffer = new Bottle[maxSize];

            //Makes threads
            Thread producer = new Thread(FillBuffer);
            producer.Name = "Producer";
            producer.Start();

            Thread SplitConsumer = new Thread(SplitBuffer);
            SplitConsumer.Name = "SplitConsumer";
            SplitConsumer.Start();

            Thread SodaConsumer = new Thread(ClearBuffer);
            SodaConsumer.Name = "SodaConsumer";
            SodaConsumer.Start(sodaBuffer);

            Thread BeerConsumer = new Thread(ClearBuffer);
            BeerConsumer.Name = "BeerConsumer";
            BeerConsumer.Start(beerBuffer);

            Console.ReadLine();
        }

        static Random random = new Random();

        static void FillBuffer()
        {
            while (true)
            {
                for (int i = 0; i < producerBuffer.Length; i++)
                {
                    //Makes new bottle object
                    Bottle bottle;
                    if (random.Next(0, 2) == 0)
                    {
                        bottle = new Bottle("Beer");
                    }
                    else
                    {
                        bottle = new Bottle("Soda");
                    }

                    //Tries to lock producerBuffer
                    Monitor.Enter(producerBuffer);
                    try
                    {
                        //While the current loop bottle is a thing, we wait
                        while (producerBuffer[i] != null)
                        {
                            Monitor.Wait(producerBuffer);
                        }

                        //When it is not a thing, we make it a thing
                        producerBuffer[i] = bottle;
                        Console.WriteLine($"Bottle Name: ".PadLeft(5) + $" {bottle.name} | Number: ".PadLeft(10) + $" {bottle.number}".PadLeft(3) + " | Thread Name: ".PadLeft(15) + $" {Thread.CurrentThread.Name}");
                        Thread.Sleep(50);


                        Monitor.PulseAll(producerBuffer);
                    }
                    finally
                    {
                        Monitor.Exit(producerBuffer);
                    }
                }
            }
        }

        static void SplitBuffer()
        {
            while (true)
            {
                Monitor.Enter(producerBuffer);
                try
                {
                    for (int i = 0; i < producerBuffer.Length; i++)
                    {
                        //Checks if the current bottle is not null
                        while (producerBuffer[i] == null)
                        {
                            Monitor.Wait(producerBuffer);
                        }

                        Bottle bottle = producerBuffer[i];
                        Console.WriteLine($"Bottle Name: ".PadLeft(5) + $" {bottle.name} | Number: ".PadLeft(10) + $" {bottle.number}".PadLeft(3) + " | Thread Name: ".PadLeft(15) + $" {Thread.CurrentThread.Name}");
                        producerBuffer[i] = null;

                        //Checks which bottle it is, put it where the bottle belongs
                        if (bottle.name == "Beer")
                        {
                            Monitor.Enter(beerBuffer);
                            try
                            {
                                for (int j = 0; j < beerBuffer.Length; j++)
                                {
                                    if (beerBuffer[i] == null)
                                    {
                                        beerBuffer[i] = bottle;
                                        break;
                                    }
                                }

                                Monitor.PulseAll(beerBuffer);
                            }
                            finally
                            {
                                Monitor.Exit(beerBuffer);
                            }
                        }
                        else
                        {
                            Monitor.Enter(sodaBuffer);
                            try
                            {
                                for (int j = 0; j < sodaBuffer.Length; j++)
                                {
                                    if (sodaBuffer[i] == null)
                                    {
                                        sodaBuffer[i] = bottle;
                                        break;
                                    }
                                }

                                Monitor.PulseAll(sodaBuffer);
                            }
                            finally
                            {
                                Monitor.Exit(sodaBuffer);
                            }
                        }
                        Thread.Sleep(500);

                        Monitor.PulseAll(producerBuffer);
                    }

                    
                }
                finally
                {
                    Monitor.Exit(producerBuffer);
                }
            }
        }

        static void ClearBuffer(object bottleArrayObj)
        {
            Bottle[] bottleArry = bottleArrayObj as Bottle[];

            while (true)
            {
                Monitor.Enter(bottleArry);
                try
                {
                    for (int i = 0; i < bottleArry.Length; i++)
                    {
                        //Checks if the specific buf
                        while (bottleArry[i] == null)
                        {
                            Monitor.Wait(bottleArry);
                        }

                        Console.WriteLine($"Bottle Name: ".PadLeft(5) + $" {bottleArry[i].name} | Number: ".PadLeft(10) + $" {bottleArry[i].number}".PadLeft(3) + " | Thread Name: ".PadLeft(15) + $" {Thread.CurrentThread.Name}");
                        bottleArry[i] = null;
                        Thread.Sleep(500);

                        Monitor.PulseAll(bottleArry);

                    }

                }
                finally
                {
                    Monitor.Exit(bottleArry);
                }
            }
        }
    }
}
