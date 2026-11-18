using System.Text;
using SmartParking.Services;
using SmartParking.UI;

Console.OutputEncoding = Encoding.UTF8;

var manager = new ParkingManager();
var menu = new ConsoleMenu(manager);
menu.Run();
