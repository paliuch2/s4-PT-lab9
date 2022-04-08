// KAMIL PALUSZEWSKI 180194

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace PT9
{
    class Program
    {
        private static List<Car> myCars = new List<Car>()
        { new Car("E250", new Engine(1.8, 204, "CGI"), 2009),
            new Car("E350", new Engine(3.5, 292, "CGI"), 2009),
            new Car("A6", new Engine(2.5, 187, "FSI"), 2012),
            new Car("A6", new Engine(2.8, 220, "FSI"), 2012),
            new Car("A6", new Engine(3.0, 295, "TFSI"), 2012),
            new Car("A6", new Engine(2.0, 175, "TDI"), 2011),
            new Car("A6", new Engine(3.0, 309, "TDI"), 2011),
            new Car("S6", new Engine(4.0, 414, "TFSI"), 2012),
            new Car("S8", new Engine(4.0, 513, "TFSI"), 2012) };



        static void Main(string[] args)
        {
            LINQqueries(); // zadanie 1.
            SerialAndDeserial(); // zadanie 2.
            XPath(); // zadanie 3.
            XMLfromLINQ(myCars); // zadanie 4.
            HTMLdocument(); // zadanie 5.
            XMLmodifications(); // zadanie 6.
        }

        private static void XMLmodifications()
        {
            XElement root = XElement.Load("CarsCollection.xml");

            foreach (var c in root.Elements())
            {
                foreach (var f in c.Elements())
                {
                    if (f.Name == "engine")
                    {
                        foreach (var e in f.Elements())
                        {
                            if (e.Name == "horsePower")
                            {
                                e.Name = "hp";                           
                            }
                        }
                    }
                    else if (f.Name == "model")
                    {
                        f.SetAttributeValue("year", c.Element("year").Value);
                        var toRemove = c.Element("year");
                        toRemove.Remove();
                    }
                }
            }

            root.Save("CarsCollectionAfterModification.xml");
        }

        private static void HTMLdocument()
        {
            XElement templ = XElement.Load("template.html");
            XElement body = templ.Element("{http://www.w3.org/1999/xhtml}body");

            IEnumerable<XElement> rows = from c in myCars
                                         select new XElement("tr",
                                                        new XElement("td", new XAttribute("style", "border: 1px double black"), c.model),
                                                        new XElement("td", new XAttribute("style", "border: 1px double black"), c.motor.model),
                                                        new XElement("td", new XAttribute("style", "border: 1px double black"), c.motor.displacement),
                                                        new XElement("td", new XAttribute("style", "border: 1px double black"), c.motor.horsePower),
                                                        new XElement("td", new XAttribute("style", "border: 1px double black"), c.year));


            body.Add(new XElement("table", new XAttribute("style", "border: 1.5px double black"), rows));
            templ.Save("carsTable.html");
        }

        private static void XMLfromLINQ(List<Car> myCars)
        {
            IEnumerable<XElement> nodes = from c in myCars
                                          select new XElement("car",
                                                    new XElement("model", c.model), new XElement("engine",
                                                            new XAttribute("model", c.motor.model), new XElement("displacement", c.motor.displacement),
                                                            new XElement("horsePower", c.motor.horsePower)),
                                                    new XElement("year", c.year));


            XElement rootNode = new XElement("cars", nodes);
            rootNode.Save("CarsFromLinq.xml");
        }

        private static void LINQqueries()
        {

            var query1 = from car in myCars
                         where car.model == "A6"
                         select new
                         {                                                
                             engineType = car.motor.model == "TDI" ? "diesel" : "petrol",

                             hppl = car.motor.horsePower / car.motor.displacement
                         };



            var query2 = from car in query1
                         group car.hppl by car.engineType;


            foreach (var e in query2)
            {
                Console.WriteLine(e.Key + ": " + e.Sum() / e.Count());
            }

        }

        private static void SerialAndDeserial()
        {
            var file = "CarsCollection.xml";
            XmlSerializer serial = new XmlSerializer(typeof(List<Car>), new XmlRootAttribute("cars"));

            using (var stream = File.OpenWrite(file))
            {
                serial.Serialize(stream, myCars);
            }

            XmlSerializer deserial = new XmlSerializer(typeof(List<Car>), new XmlRootAttribute("cars"));
            List<Car> deserializedCars = null;
            using (XmlReader reader = XmlReader.Create("CarsCollection.xml"))
            {
                deserializedCars = (List<Car>)deserial.Deserialize(reader);
            }

            Console.WriteLine("\nSamochody po deserializacji: ");
            foreach (var car in deserializedCars)
            {
                Console.WriteLine(car.model + " " + car.year + " " + car.motor.displacement);
            }
        }

        private static void XPath()
        {

            XElement rootNode = XElement.Load("CarsCollection.xml");
            double avgHP = (double)rootNode.XPathEvaluate("sum(//car/engine[@model!=\"TDI\"]/horsePower) div count(//car/engine[@model!=\"TDI\"]/horsePower)");
            Console.Write("\nPrzecietna moc samochodow o silnikach innych niz TDI: {0}\n\n", avgHP);

            IEnumerable<XElement> models = rootNode.XPathSelectElements("//car/model[not(. = preceding::model)]");
            Console.Write("Modele samochodow bez powtorzen: ");

            foreach (var model in models)
            {
                Console.Write(model.Value + " ");
            }
        }

    }

    [XmlType("car")]
    public class Car
    {
        public string model;
        public int year;
        [XmlElement("engine")]
        public Engine motor;

        public Car() { }

        public Car(string model, Engine motor, int year)
        {
            this.model = model;
            this.year = year;
            this.motor = motor;
        }

    }

    public class Engine
    {
        public double displacement;
        public double horsePower;
        [XmlAttribute]
        public string model;

        public Engine() { }

        public Engine(double displacement, double horsePower, string model)
        {
            this.displacement = displacement;
            this.horsePower = horsePower;
            this.model = model;
        }


    }
}
