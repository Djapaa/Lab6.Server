using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml.Serialization;
using System.Threading;
using System.Collections.Generic;
using Lab6.Server;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Reflection;
using System.Linq;
using System.Text;

[Serializable]
[DataContract]
public struct Date
{
    [DataMember]
    public int year;
    [DataMember]
    public int month;
    [DataMember]
    public int day;

    
    public Date(int year, int month, int day)
    {
        this.year = year;
        this.month = month;
        this.day = day;
    }
   

    public void DisplayInfo()
    {
        Console.WriteLine($"year: {year}  month: {month} day:{day}");
    }
}


   


public class UdpFileServer
{
    
    // Информация о файле (требуется для получателя)
    [Serializable]
    public class FeleInfo
    {
        public string FILETYPE = "";
        public long FILESIZE = 0;
    }

    private static FeleInfo fileInfo = new FeleInfo();

    // Поля, связанные с UdpClient
    private static IPAddress remoteIPAddress;
    private const int remotePort = 5002;
    private static UdpClient sender = new UdpClient();
    private static IPEndPoint endPoint;

    // Filestream object
    private static FileStream fs;

    [STAThread]
    static void Main(string[] args)
    {
        Random rnd = new Random();
        var persons = new List<Person>();

        for (int i = 1; i < 10; i++)
        {       
            Date date = new Date(rnd.Next(1900,2021),rnd.Next(1,12),rnd.Next(1,31));
            int age = 2021 - date.year;
            persons.Add(new Person("Номер человека в списке:" + i, age,date));           
        }


        //сериализация
        var binFormatter = new BinaryFormatter();
        using (var fileBin = new FileStream("Binary.bin", FileMode.OpenOrCreate))
        {
            binFormatter.Serialize(fileBin, persons);

        }
        var xmlFormatter = new XmlSerializer(typeof(List<Person>));
        using (var fileXml = new FileStream("XML.xml", FileMode.OpenOrCreate))
        {
            xmlFormatter.Serialize(fileXml, persons);
        }
        var jsonFormatter = new DataContractJsonSerializer(typeof(List<Person>));
        using (var fileJson = new FileStream("JSON.json", FileMode.OpenOrCreate))

        {
            jsonFormatter.WriteObject(fileJson,persons);
        }
       
        #region передача данных
        try
        {
                // Получаем удаленный IP-адрес и создаем IPEndPoint
                Console.WriteLine("Введите удаленный IP-адрес");
                remoteIPAddress = IPAddress.Parse(Console.ReadLine().ToString());//"127.0.0.1");
                endPoint = new IPEndPoint(remoteIPAddress, remotePort);

                // Получаем путь файла и его размер (должен быть меньше 8kb)
                Console.WriteLine("Введите путь к файлу и его имя");
                fs = new FileStream(@Console.ReadLine().ToString(), FileMode.Open, FileAccess.Read);

                if (fs.Length > 8192)
                {
                    Console.Write("Файл должен весить меньше 8кБ");
                    sender.Close();
                    fs.Close();
                    return;
                }

                // Отправляем информацию о файле
                SendFileInfo();

                // Ждем 2 секунды
                Thread.Sleep(2000);

                // Отправляем сам файл
                SendFile();

                Console.ReadLine();

            }
            catch (Exception eR)
            {
                Console.WriteLine(eR.ToString());
            }

    }
    public static void SendFileInfo()
    {

        // Получаем тип и расширение файла
        fileInfo.FILETYPE = fs.Name.Substring((int)fs.Name.Length - 3, 3);

        // Получаем длину файла
        fileInfo.FILESIZE = fs.Length;

        XmlSerializer fileSerializer = new XmlSerializer(typeof(FeleInfo));
        MemoryStream stream = new MemoryStream();

        // Сериализуем объект
        fileSerializer.Serialize(stream, fileInfo);

        // Считываем поток в байты
        stream.Position = 0;
        Byte[] bytes = new Byte[stream.Length];
        stream.Read(bytes, 0, Convert.ToInt32(stream.Length));

        Console.WriteLine("Отправка деталей файла...");

        // Отправляем информацию о файле
        sender.Send(bytes, bytes.Length, endPoint);
        stream.Close();

    }
    private static void SendFile()
    {
        // Создаем файловый поток и переводим его в байты
        Byte[] bytes = new Byte[fs.Length];
        fs.Read(bytes, 0, bytes.Length);

        Console.WriteLine("Отправка файла размером " + fs.Length + " байт");
        try
        {
            // Отправляем файл
            sender.Send(bytes, bytes.Length, endPoint);
        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
        finally
        {
            // Закрываем соединение и очищаем поток
            fs.Close();
            sender.Close();
        }
        Console.WriteLine("Файл успешно отправлен.");
        Console.Read();
    }
    #endregion
}
