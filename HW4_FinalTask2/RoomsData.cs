using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW4_FinalTask2
{
    public class RoomsData
    {
        #region class member variables
        Autodesk.Revit.UI.UIApplication m_revit;  // Store the reference of the application in revit

        List<Room> m_rooms = new List<Room>();    // a list to store all rooms in the project
        List<RoomTag> m_roomTags = new List<RoomTag>(); // a list to store all room tags
        List<Room> m_roomsWithTag = new List<Room>();   // a list to store all rooms with tag
        List<Room> m_roomsWithoutTag = new List<Room>(); // a list to store all rooms without tag
        #endregion


        /// <summary>
        /// a list of all the rooms in the project
        /// </summary>
        public List<Room> Rooms
        {
            get
            {
                return new List<Room>(m_rooms);
            }
        }


        /// <summary>
        /// a list of all the room tags in the project
        /// </summary>
        public List<RoomTag> RoomTags
        {
            get
            {
                return new List<RoomTag>(m_roomTags);
            }
        }


        /// <summary>
        /// a list of the rooms that had tag
        /// </summary>
        public List<Room> RoomsWithTag
        {
            get
            {
                return new List<Room>(m_roomsWithTag);
            }
        }


        /// <summary>
        /// a list of the rooms which lack room tag
        /// </summary>
        public List<Room> RoomsWithoutTag
        {
            get
            {
                return new List<Room>(m_roomsWithoutTag);
            }
        }


        /// <summary>
        ///constructor 
        /// </summary>
        public RoomsData(ExternalCommandData commandData)
        {
            m_revit = commandData.Application;

            // get all the rooms and room tags in the project
            GetAllRoomsAndTags();

            // find out the rooms that without room tag
            ClassifyRooms();

            // create the room tags for the rooms which lack room tag
            CreateTags();
        }


        /// <summary>
        /// create the room tags for the rooms which lack room tag
        /// </summary>
        public void CreateTags()
        {
            try
            {
                foreach (Room tmpRoom in m_roomsWithoutTag)
                {
                    // get the location point of the room
                    LocationPoint locPoint = tmpRoom.Location as LocationPoint;
                    if (null == locPoint)
                    {
                        String roomId = "Room Id:  " + tmpRoom.Id.IntegerValue.ToString();
                        String errMsg = roomId + "\r\nFault to create room tag," +
                                                   "can't get the location point!";
                        throw new Exception(errMsg);
                    }

                    // create a instance of Autodesk.Revit.DB.UV class
                    UV point = new UV(locPoint.Point.X, locPoint.Point.Y);

                    //create room tag
                    RoomTag tmpTag;
                    tmpTag = m_revit.ActiveUIDocument.Document.Create.NewRoomTag(new LinkElementId(tmpRoom.Id), point, null);
                    if (null != tmpTag)
                    {
                        m_roomTags.Add(tmpTag);
                    }
                }

                // classify rooms
                ClassifyRooms();

                // display a message box
                TaskDialog.Show("Revit", "Add room tags complete!");
            }
            catch (Exception exception)
            {
                TaskDialog.Show("Revit", exception.Message);
            }
        }


        /// <summary>
        /// get all the rooms and room tags in the project
        /// </summary>
        private void GetAllRoomsAndTags()
        {
            // get the active document 
            Document document = m_revit.ActiveUIDocument.Document;
            RoomFilter roomFilter = new RoomFilter();
            RoomTagFilter roomTagFilter = new RoomTagFilter();
            LogicalOrFilter orFilter = new LogicalOrFilter(roomFilter, roomTagFilter);

            FilteredElementIterator elementIterator =
                (new FilteredElementCollector(document)).WherePasses(orFilter).GetElementIterator();
            elementIterator.Reset();

            // try to find all the rooms and room tags in the project and add to the list
            while (elementIterator.MoveNext())
            {
                object obj = elementIterator.Current;

                // find the rooms, skip those rooms which don't locate at Level yet.
                Room tmpRoom = obj as Room;
                if (null != tmpRoom && null != document.GetElement(tmpRoom.LevelId))
                {
                    m_rooms.Add(tmpRoom);
                    continue;
                }

                // find the room tags
                RoomTag tmpTag = obj as RoomTag;
                if (null != tmpTag)
                {
                    m_roomTags.Add(tmpTag);
                    continue;
                }
            }
        }


        /// <summary>
        /// find out the rooms that without room tag
        /// </summary>
        private void ClassifyRooms()
        {
            m_roomsWithoutTag.Clear();
            m_roomsWithTag.Clear();

            // copy the all the elements in list Rooms to list RoomsWithoutTag
            m_roomsWithoutTag.AddRange(m_rooms);

            // get the room id from room tag via room property
            // if find the room id in list RoomWithoutTag,
            // add it to the list RoomWithTag and delete it from list RoomWithoutTag
            foreach (RoomTag tmpTag in m_roomTags)
            {
                int idValue = tmpTag.Room.Id.IntegerValue;
                m_roomsWithTag.Add(tmpTag.Room);

                // search the id for list RoomWithoutTag
                foreach (Room tmpRoom in m_rooms)
                {
                    if (idValue == tmpRoom.Id.IntegerValue)
                    {
                        m_roomsWithoutTag.Remove(tmpRoom);
                    }
                }
            }
        }

    }

}
