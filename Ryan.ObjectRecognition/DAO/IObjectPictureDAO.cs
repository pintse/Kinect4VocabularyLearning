using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Ryan.ObjectRecognition.VO;

namespace Ryan.ObjectRecognition.DAO
{
    /// <summary>
    /// 物件圖片資料存取程式實做介面
    /// </summary>
    public interface IObjectPictureDAO
    {
        Dictionary<string, ObjectPictureVO> getAllPictures();
        Dictionary<string, ObjectPictureVO> getNewPictures();
        Bitmap getPicture(ObjectPictureVO objectPictureVO);
        void moveNewPicture(ObjectPictureVO objectPictureVO);

        Dictionary<string, CongruousObjectVO> fillPictures2Objects(Dictionary<string, CongruousObjectVO> candidateObjects);
    }
}
