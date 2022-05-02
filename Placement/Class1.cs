using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Placement
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class TagRooms : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            List<Room> rooms = new List<Room>();
            List<Level> levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .OfType<Level>()
                .ToList();
            Transaction ts = new Transaction(doc, "Создание комнат");
            ts.Start();
            foreach (var level in levels)
            {
                doc.Create.NewRooms2(level);
            }
            ts.Commit();

            rooms = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .OfType<Room>()
                .ToList();
            Transaction ts2 = new Transaction(doc, "Создание tag");
            ts2.Start();
            int roomcounter = 0;
            FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc);
            filteredElementCollector.OfClass(typeof(FamilySymbol));
            filteredElementCollector.OfCategory(BuiltInCategory.OST_RoomTags);
            var roomtagtypes = filteredElementCollector.Cast<RoomTagType>().ToList<RoomTagType>();
            foreach (var room in rooms)
            {
                roomcounter++;
                string lName = room.Level.Name.Substring(6);
                room.Name = $"{lName}_{room.Number}";
                LocationPoint locationPoint = room.Location as LocationPoint;
                UV point = new UV(locationPoint.Point.X, locationPoint.Point.Y);
                RoomTag newTag = doc.Create.NewRoomTag(new LinkElementId(room.Id), point, null);
            }
            ts2.Commit();
            return Result.Succeeded;
        }
    }
}
