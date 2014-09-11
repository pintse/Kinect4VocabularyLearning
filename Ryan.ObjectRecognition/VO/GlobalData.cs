using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ryan.ObjectRecognition.VO
{
    /// <summary>
    /// ObjectRecognition全域資料
    /// </summary>
    class GlobalData
    {
        private static GlobalData _GlobalData ;
        private const string ROOT_FILE_PATH = "D:/Kinect4VocabularyLearning/Object";
        //private static string ROOT_FILE_PATH = Path.Combine(Environment.CurrentDirectory);
        private const string PREFIX_FILE_FOLDER = "ObjectRecognition";
        private const string NEW_IMAGE_FOLDER = "New";
        private const string FINISH_IMAGE_FOLDER = "Finish";

        public const string FOLDER_PATH_DELIMITER = "/";
        public const string FULL_NEW_IMAGE_PATH = ROOT_FILE_PATH + FOLDER_PATH_DELIMITER + PREFIX_FILE_FOLDER + FOLDER_PATH_DELIMITER + NEW_IMAGE_FOLDER + FOLDER_PATH_DELIMITER;
        public const string IMAGE_NANE_DELIMITER = "_";
        public const string FULL_FINISH_IMAGE_PATH = ROOT_FILE_PATH + FOLDER_PATH_DELIMITER + PREFIX_FILE_FOLDER + FOLDER_PATH_DELIMITER + FINISH_IMAGE_FOLDER + FOLDER_PATH_DELIMITER;
        public const string IMAGE_EXTEND_NANE = "png";
        public static int NEW_IMAGE_THREAD_CONUNT=0;


        private GlobalData()
        {
        }

        /// <summary>
        /// 第一次取本物件須使用本方法
        /// </summary>
        /// <param name="CandidateObjectList"></param>
        /// <returns></returns>
        public static GlobalData getInstance(Dictionary<string, CongruousObjectVO> CandidateObjectList)
        {
            if (_GlobalData == null)
            {
                _GlobalData = new GlobalData();
                _GlobalData._CandidateObjects  = CandidateObjectList;
            }
            return _GlobalData;
        }

        /// <summary>
        /// 第二次以後取本物件可使用本方法
        /// </summary>
        /// <returns></returns>
        public static GlobalData getInstance()
        {
            if (_GlobalData == null)
            {
                //throw new Exception("Please call \"public static GlobalData getInstance(List<CongruousObjectVO> CandidateObjectList)\" to get this object!!");
            }
            return _GlobalData;
        }

        private Dictionary<string, CongruousObjectVO> _CandidateObjects;

        internal Dictionary<string, CongruousObjectVO> CandidateObjects
        {
            get { return _CandidateObjects; }
        }



    }
}
