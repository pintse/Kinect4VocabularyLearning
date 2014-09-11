using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Ryan.ObjectRecognition.SURF;
using Ryan.ObjectRecognition.VO;
using Ryan.ObjectRecognition.DAO;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Ryan.Common.Service;
using Ryan.Common.DAO;

namespace Ryan.ObjectRecognition.Service
{
    /// <summary>
    /// 物件辨識流程控制
    /// </summary>
    class RecongitionHandler
    {
        private static volatile RecongitionHandler _Myself;
        private static readonly object ticket = new object();
        private static ILog log = LogManager.GetLogger(typeof(RecongitionHandler));

        private RecongitionCollection _RecongitionCollection;

        private RecongitionHandler(RecongitionCollection recongitionProcessor)
        {
            _RecongitionCollection = recongitionProcessor;
        }

        public static RecongitionHandler getInstance(RecongitionCollection recongitionProcessor)
        {
            if (_Myself == null)
            {
                lock (ticket)
                {
                    if (_Myself == null)
                    {
                        _Myself = new RecongitionHandler(recongitionProcessor);

                    }
                }
                
            }
            return _Myself;
        }

        public CongruousObjectVO recognizeObjectCongruous(Bitmap realimagemap)
        {
            return _RecongitionCollection.ResultProcessor.determine(recognizeCongruousObjects(realimagemap)); 
        }

        public string recognizeObject(Bitmap realimagemap)
        {
            log.Info("物件即時辨識開始...");

            CongruousObjectVO congruousObjectVO = null;

            congruousObjectVO = recognizeObjectCongruous(realimagemap); 
            ImageDAO.getInstance().saveBitmap(realimagemap, congruousObjectVO.ObjectPicture.ObjectName); ///TODO for testing

            if (congruousObjectVO.ObjectPicture != null)
            {
                log.Info("＃＃＃物件即時辨識結果::ObjectId::" + congruousObjectVO.ObjectPicture.ObjectId + ", ObjectPictureId::" + congruousObjectVO.ObjectPicture.ObjectPictureId +
                    ", ObjectName::" + congruousObjectVO.ObjectPicture.ObjectName);

                log.Info(", Color::" + ((congruousObjectVO.RecognitionScoreSet.ContainsKey("Color")) ? congruousObjectVO.RecognitionScoreSet["Color"] + "" : "") +
                    ", SURF::" + ((congruousObjectVO.RecognitionScoreSet.ContainsKey("SURF")) ? congruousObjectVO.RecognitionScoreSet["SURF"] + "" : ""));

                double score = 0;

                foreach (var data in congruousObjectVO.RecognitionScoreSet)
                {
                    score += data.Value;
                }

                if (score >= 3)
                {
                    return congruousObjectVO.ObjectPicture.ObjectId;
                }
                else
                {
                    log.Info("物件即時辨識結果符合度太低...");
                    return "物件即時辨識結果無符合";
                }
            }
            else
            {
                log.Info("物件即時辨識結果無符合...");
                return "物件即時辨識結果無符合";
            }     

        }

        public Dictionary<string, CongruousObjectVO> recognizeCongruousObjects(Bitmap realimagemap)
        {

            Dictionary<string, CongruousObjectVO> candidateObjects = GlobalData.getInstance().CandidateObjects;
            checkCandidateObjects(" GlobalData.getInstance().CandidateObjects", candidateObjects);  //TODO for debug

            int i = 0;
            foreach (IRecongitionProcessor recongitionProcessor in _RecongitionCollection.RecongitionProcessors)
            {
                log.Debug("執行第"+ ++i +"種辨識方式開始");
                candidateObjects = recongitionProcessor.getBestCongruousObjects(realimagemap, candidateObjects);

                checkCandidateObjects("" + recongitionProcessor.GetType()+"-"+recongitionProcessor.ToString(), candidateObjects);  //TODO for debug

                //Boolean sameFlg = true;
                string lastObjectId = "";
                string picture_id = "";
                foreach (KeyValuePair<string, CongruousObjectVO> kvp in candidateObjects)
                {
                    try
                    {
                        picture_id += kvp.Value.ObjectPicture.ObjectId + GlobalData.IMAGE_NANE_DELIMITER + kvp.Value.ObjectPicture.ObjectPictureId + ", ";
                        if (lastObjectId == "")
                        {
                            lastObjectId = kvp.Value.ObjectPicture.ObjectId;
                        }
                        else
                        {
                            if (lastObjectId != kvp.Value.ObjectPicture.ObjectId)
                            {
                                //sameFlg = false;
                                lastObjectId = kvp.Value.ObjectPicture.ObjectId;
                            }
                        }
                    }catch(Exception ex){
                        StringBuilder message = new StringBuilder();
                        if (kvp.Value == null)
                        {
                            message.Append("kvp.Value is null,");
                        }
                        else if (kvp.Value.ObjectPicture == null)
                        {
                            message.Append("kvp.Value.ObjectPicture is null,");
                        }
                        else
                        {
                            if (kvp.Value.ObjectPicture.ObjectId == null)
                                message.Append("kvp.Value.ObjectPicture.ObjectId is null,");
                            if (kvp.Value.ObjectPicture.ObjectPictureId == null)
                                message.Append("kvp.Value.ObjectPicture.ObjectPictureId is null,");
                        }

                        log.Fatal(message.ToString()+"--"+ ex);
                        Console.WriteLine(message.ToString() + "--" + ex);
                        throw ex;
                    }
                }
                log.Debug("得到的候選筆數::" + candidateObjects.Count + ", list::" + picture_id);
                log.Debug("執行第" + i + "種辨識方式結束");

                /*if (sameFlg)
                {
                    break;
                }*/
            }

            return candidateObjects;

        }

        
        //////////////////////////////////////////////////////////////////////////

        #region 建新照片特徵資料
        
        
        

        public void processNewPictures(IObjectPictureDAO objectPictureDAO, ObjectMainDAO objectMainDAO)
        {
            try
            {
                log.Info("RecongitionHandler.processNewPictures...");

                Dictionary<string, ObjectPictureVO> objectPictureVOs = objectPictureDAO.getNewPictures();

                processNewPicturesFeature(objectMainDAO, objectPictureVOs, objectPictureDAO);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                //Console.WriteLine(ex.Message);
            }
        }

        private void processNewPicturesFeature(ObjectMainDAO objectMainDAO, Dictionary<string, ObjectPictureVO> objectPictureVOs, IObjectPictureDAO objectPictureDAO)
        {
            Dictionary<string, string> objectIdMaps = new Dictionary<string, string>();
            string previousObjFolder = "";

            foreach (KeyValuePair<string, ObjectPictureVO> kvp in objectPictureVOs)
            {
                Boolean folderChangeFlag = false;
                if (kvp.Value.ObjectId != previousObjFolder)
                {
                    previousObjFolder = kvp.Value.ObjectId;
                    folderChangeFlag = true;
                }
                else
                {
                    kvp.Value.ObjectId = objectIdMaps[kvp.Value.ObjectId];
                }

                //建立基本資料
                objectMainDAO.createNewImageMainFile(kvp.Value);  //寫入物件資料，到table和物件本身

                if (folderChangeFlag)
                {
                    objectIdMaps.Add(previousObjFolder, kvp.Value.ObjectId);
                }

            }

            //Console.WriteLine("＃＃＃＃＃＃＃＃＃＃辨識特徵開始！！！＃＃＃＃＃＃＃＃＃＃" + DateTime.Now.ToString());
            log.Info("＃＃＃＃＃＃＃＃＃＃辨識特徵開始！！！＃＃＃＃＃＃＃＃＃＃");

            processNewPicturesParallel(objectPictureVOs, objectPictureDAO);

            //Console.WriteLine("＃＃＃＃＃＃＃＃＃＃新圖片建檔完成！！！＃＃＃＃＃＃＃＃＃＃"+DateTime.Now.ToString());
            log.Info("＃＃＃＃＃＃＃＃＃＃新圖片建檔完成！！！＃＃＃＃＃＃＃＃＃＃");
        }

        private void processNewPicturesParallel(Dictionary<string, ObjectPictureVO> objectPictureVOs, IObjectPictureDAO objectPictureDAO)
        {
            Parallel.ForEach(objectPictureVOs, (item, loopState) =>
            {
                processNewPictureParallelMethod(item, objectPictureDAO);
                
            });
        }

        private void processNewPicturesThread(KeyValuePair<string, ObjectPictureVO> kvp)
        {
            ParameterizedThreadStart ParStart = new ParameterizedThreadStart(processNewPictureThreadMethod);
            Thread processNewPicturesThread = new Thread(ParStart);

            GlobalData.NEW_IMAGE_THREAD_CONUNT++;
            
            processNewPicturesThread.Start(kvp);  // 開始執行 這個執行緒

        }


        private void processNewPictureParallelMethod(KeyValuePair<string, ObjectPictureVO> kvp, IObjectPictureDAO objectPictureDAO)
        {
            log.Info("＃＃＃＃＃＃＃＃＃＃新圖片建檔Thread開始！！！＃＃＃＃＃＃＃＃＃＃");

            //KeyValuePair<string, ObjectPictureVO> kvp = (KeyValuePair<string, ObjectPictureVO>)o;

            //建立特徵資料
            foreach (IRecongitionProcessor rp in _RecongitionCollection.RecongitionProcessors)
            {
                rp.buildNewImageFeaturePersistence(kvp.Value);
            }

            kvp.Value.ObjectBitmap.Dispose();
            kvp.Value.ObjectBitmap = null;
            objectPictureDAO.moveNewPicture(kvp.Value);

            //kvp.Value.clear();

            GlobalData.NEW_IMAGE_THREAD_CONUNT--;
            //Console.WriteLine(Thread.CurrentThread.ManagedThreadId + "＃＃＃＃＃＃＃＃＃＃新圖片建檔Thread完成！！！＃＃＃＃＃＃＃＃＃＃" + DateTime.Now.ToString());
            log.Info("＃＃＃＃＃＃＃＃＃＃新圖片建檔Thread完成！！！＃＃＃＃＃＃＃＃＃＃");
        }

        private void processNewPictureThreadMethod(Object o)
        {
            log.Info("＃＃＃＃＃＃＃＃＃＃新圖片建檔Thread開始！！！＃＃＃＃＃＃＃＃＃＃");

            KeyValuePair<string, ObjectPictureVO> kvp = (KeyValuePair<string, ObjectPictureVO>)o;

            //建立特徵資料
            foreach (IRecongitionProcessor rp in _RecongitionCollection.RecongitionProcessors)
            {
                rp.buildNewImageFeaturePersistence(kvp.Value);
            }

            //objectPictureDAO.moveNewPicture(kvp.Value);

            //kvp.Value.clear();

            GlobalData.NEW_IMAGE_THREAD_CONUNT--;
            //Console.WriteLine(Thread.CurrentThread.ManagedThreadId + "＃＃＃＃＃＃＃＃＃＃新圖片建檔Thread完成！！！＃＃＃＃＃＃＃＃＃＃" + DateTime.Now.ToString());
            log.Info("＃＃＃＃＃＃＃＃＃＃新圖片建檔Thread完成！！！＃＃＃＃＃＃＃＃＃＃");
        }

        #endregion

        //////////////////////////////////////////
        #region 測試區塊

        private void checkCandidateObjects(String comeFrom, Dictionary<string, CongruousObjectVO> candidateObjects)
        {
            int count = candidateObjects.Count;
            int i = 0;
            foreach (KeyValuePair<string, CongruousObjectVO> kvp in candidateObjects)
            {
                i++;
                if (kvp.Value == null)
                {
                    log.Fatal(comeFrom + "給的candidateObjects有" + count + "筆，第" + i + "筆含Null，Key::" + kvp.Key);
                }
            }
        }

        #endregion
    }

}
