using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ryan.ObjectRecognition.VO;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using log4net;

namespace Ryan.ObjectRecognition.DAO
{
    public class ObjectPictureDAO : IObjectPictureDAO
    {
        private static ObjectPictureDAO _Myself = new ObjectPictureDAO();
        private static ILog log = LogManager.GetLogger(typeof(ObjectPictureDAO));

        private ObjectPictureDAO() { }

        public static ObjectPictureDAO getInstance()
        {
            return _Myself;
        }


        public Dictionary<string, ObjectPictureVO> getAllPictures()
        {
            return getPictures(GlobalData.FULL_FINISH_IMAGE_PATH, null);
        }

        public Dictionary<string, ObjectPictureVO> getNewPictures()
        {
            return getPictures(GlobalData.FULL_NEW_IMAGE_PATH, null);
        }

        private Dictionary<string, ObjectPictureVO> getPictures(string filepath, Dictionary<string, ObjectPictureVO> ObjectPictureVOs)
        {
            log.Info("getPictures() start...");
            if (ObjectPictureVOs == null)
            {
                ObjectPictureVOs = new Dictionary<string, ObjectPictureVO>();
            }

            int i;
            try
            {
                string[] oDirectorys = Directory.GetDirectories(filepath);
                for (i = 0; i < oDirectorys.Length; i++)
                    getPictures(oDirectorys[i], ObjectPictureVOs);
            }
            catch(Exception ex) 
            {
                log.Fatal(ex);
                throw ex;
            }
            try
            {
                log.Info("filepath::" + filepath);
                string folder = filepath.Substring(filepath.LastIndexOf("/")+1);
                string[] oFiles = System.IO.Directory.GetFiles(filepath);
                for (i = 0; i < oFiles.Length; i++)
                {

                    //log.Info("folder::" + folder);
                    oFiles[i] = oFiles[i].Replace("\\","/");
                    log.Info("oFiles[" + i + "]::" + oFiles[i]);
                    string fileName = oFiles[i].Substring(oFiles[i].LastIndexOf("\\")+1);
                    fileName = Regex.Replace(fileName, "." + GlobalData.IMAGE_EXTEND_NANE, "", RegexOptions.IgnoreCase);
                    //log.Info("fileName::" + fileName);
                    
                    ObjectPictureVO vo = new ObjectPictureVO();
                    vo.ObjectId = folder;
                    vo.ObjectName = folder;
                    vo.ObjectPictureId = fileName;
                    vo.ObjectBitmap = new Bitmap(oFiles[i]);
                    vo.ExtendPath = oFiles[i];

                    ObjectPictureVOs.Add(folder+":"+fileName, vo);
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                //Console.WriteLine(ex.Message);
            }

            log.Info("getPictures() end...");

            return ObjectPictureVOs;
        }

        public void moveNewPicture(ObjectPictureVO objectPictureVO)
        {
            string path = objectPictureVO.ExtendPath;
            string path2 = GlobalData.FULL_FINISH_IMAGE_PATH + objectPictureVO.ObjectId + GlobalData.FOLDER_PATH_DELIMITER + objectPictureVO.ObjectId + GlobalData.IMAGE_NANE_DELIMITER + objectPictureVO.ObjectPictureId + "." + GlobalData.IMAGE_EXTEND_NANE;
            log.Info("原始檔案路徑::" + path);
            log.Info("最終檔案路徑::" + path2);
            try
            {
                if (!File.Exists(path))
                {
                    // This statement ensures that the file is created,
                    // but the handle is not kept.
                    using (FileStream fs = File.Create(path)) { }
                }

                // Ensure that the target does not exist.
                string dir = path2.Substring(0, path2.LastIndexOf(GlobalData.FOLDER_PATH_DELIMITER));
                log.Info("將搬移至目標dir::" + dir);

                lock (this)
                {
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }

                if (File.Exists(path2))
                    File.Delete(path2);

                // Move the file.
                File.Move(path, path2);
                log.Info(path + " was moved to " + path2);

                // See if the original exists now.
                if (File.Exists(path))
                {
                    log.Info("The original file still exists, which is unexpected.");
                }
                else
                {
                    log.Info("The original file no longer exists, which is expected.");
                }

            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                //Console.WriteLine("The process failed: {0}", ex.ToString());
            }
        
        }

        public Bitmap getPicture(ObjectPictureVO objectPictureVO)
        {
            string fileName = GlobalData.FULL_FINISH_IMAGE_PATH + objectPictureVO.ObjectId + GlobalData.FOLDER_PATH_DELIMITER + objectPictureVO.ObjectId + GlobalData.IMAGE_NANE_DELIMITER + objectPictureVO.ObjectPictureId + "." + GlobalData.IMAGE_EXTEND_NANE;
            if (System.IO.File.Exists(fileName)) //檔名存在
            {
                // Read byte[] from png file
                BinaryReader binReader = new BinaryReader(File.Open(fileName, FileMode.Open));
                FileInfo fileInfo = new FileInfo(fileName);
                byte[] bytes = binReader.ReadBytes((int)fileInfo.Length);
                binReader.Close();

                
                Bitmap bitmap = new Bitmap(new MemoryStream(bytes));

                binReader.Close();
                binReader.Dispose();
                binReader = null;

                return bitmap;
               
            }
            else
            {
                throw new Exception("檔案不存在::" + fileName);
            }
        }

        public Bitmap getPicture(String fileName)
        {   
            
            if (System.IO.File.Exists(fileName)) //檔名存在
            {
                // Read byte[] from png file
                BinaryReader binReader = new BinaryReader(File.Open(fileName, FileMode.Open));
                FileInfo fileInfo = new FileInfo(fileName);
                byte[] bytes = binReader.ReadBytes((int)fileInfo.Length);
                binReader.Close();


                Bitmap bitmap = new Bitmap(new MemoryStream(bytes));

                binReader.Close();
                binReader.Dispose();
                binReader = null;

                return bitmap;

            }
            else
            {
                throw new Exception("檔案不存在::" + fileName);
            }
        }

        public Dictionary<string, CongruousObjectVO> fillPictures2Objects(Dictionary<string, CongruousObjectVO> candidateObjects)
        {
            foreach( KeyValuePair<string, CongruousObjectVO> kvp in candidateObjects)
            {
                kvp.Value.ObjectPicture.ObjectBitmap = getPicture(kvp.Value.ObjectPicture);
            }
            return candidateObjects;
        }
    }
}
