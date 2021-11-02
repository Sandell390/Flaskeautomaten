using System;
using System.Collections.Generic;
using System.Threading;

namespace Flaskeautomaten
{
    class Program
    {
        static Queue<Bottle> producerBuffer;
        static Queue<Bottle> sodaBuffer;
        static Queue<Bottle> beerBuffer;

        static void Main(string[] args)
        {
            int maxSize = 5;

            producerBuffer = new Queue<Bottle>(maxSize);
            sodaBuffer = new Queue<Bottle>(maxSize);
            beerBuffer = new Queue<Bottle>(maxSize);

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
                List<Bottle> newBottles = new List<Bottle>();

                for (int i = 0; i < 5; i++)
                {
                    if (random.Next(0,2) == 0)
                    {
                        newBottles.Add(new Bottle("Beer"));
                    }
                    else
                    {
                        newBottles.Add(new Bottle("Soda"));
                    }
                }

                for (int i = 0; i < newBottles.Count; i++)
                {
                    lock (producerBuffer)
                    {
                        while (producerBuffer.Count > 1)
                        {
                            Monitor.Wait(producerBuffer);
                        }

                        producerBuffer.Enqueue(newBottles[i]);
                        Console.WriteLine($"Producer Name: {newBottles[i].name} | Number: {newBottles[i].number} | Thread Name: {Thread.CurrentThread.Name}");
                        Thread.Sleep(500);


                        Monitor.PulseAll(producerBuffer);
                    }
                }
            }
        }

        static void SplitBuffer()
        {
            while (true)
            {

                
                lock (producerBuffer)
                {
                    while (producerBuffer.Count == 0)
                    {
                        Monitor.Wait(producerBuffer);
                    }
                    Bottle bottle = producerBuffer.Peek();
                    Console.WriteLine($"SplitConsumer Name: {bottle.name} | Number: {bottle.number} | Thread Name: {Thread.CurrentThread.Name}");
                    producerBuffer.Dequeue();

                    if (bottle.name == "Beer")
                    {
                        lock (beerBuffer)
                        {
                            beerBuffer.Enqueue(bottle);

                            Monitor.PulseAll(beerBuffer);
                        }
                    }
                    else
                    {
                        lock (sodaBuffer)
                        {
                            sodaBuffer.Enqueue(bottle);

                            Monitor.PulseAll(sodaBuffer);
                        }
                    }


                    Thread.Sleep(500);

                    Monitor.PulseAll(producerBuffer);
                }

                
            }
        }

        static void ClearBuffer(object bottleQueueObj)
        {
            Queue<Bottle> bottleQueue = (Queue<Bottle>)(bottleQueueObj);

            while (true)
            {

                lock (bottleQueue)
                {
                    while (bottleQueue.Count == 0)
                    {
                        Monitor.Wait(bottleQueue);
                    }

                    Console.WriteLine($"Consumer Name: {bottleQueue.Peek().name} | Number: {bottleQueue.Peek().number} | Thread Name: {Thread.CurrentThread.Name}");
                    bottleQueue.Dequeue();
                    Thread.Sleep(500);

                    Monitor.PulseAll(bottleQueue);
                }
            }
        }
    }
}
