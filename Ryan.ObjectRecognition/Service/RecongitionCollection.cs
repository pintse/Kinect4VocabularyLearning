using log4net;
using Ryan.Common;
using Ryan.ObjectRecognition.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ryan.ObjectRecognition.Service
{
    /// <summary>
    /// 物件辨識整套辨識邏輯與結果處理
    /// </summary>
    class RecongitionCollection
    {

        private static volatile RecongitionCollection _Myself;
        private static readonly object ticket = new object();
        private static ILog log = LogManager.GetLogger(typeof(RecongitionCollection));

        public List<IRecongitionProcessor> RecongitionProcessors;
        public IRecongitionResultProcessor ResultProcessor;
        public List<IObjectFeatureDAO> ObjectFeatureDAOs;

        private RecongitionCollection(List<IRecongitionProcessor> recongitionProcessors, IRecongitionResultProcessor recongitionResultProcessor, List<IObjectFeatureDAO> objectFeatureDAOs)
        {
            RecongitionProcessors = recongitionProcessors;
            ResultProcessor = recongitionResultProcessor;
            ObjectFeatureDAOs = objectFeatureDAOs;
        }

        public static RecongitionCollection getInstance(List<IRecongitionProcessor> recongitionProcessors, IRecongitionResultProcessor recongitionResultProcessor, List<IObjectFeatureDAO> objectFeatureDAOs)
        {
            if (_Myself == null)
            {
                lock (ticket)
                {
                    if (_Myself == null)
                    {
                        _Myself = new RecongitionCollection(recongitionProcessors, recongitionResultProcessor, objectFeatureDAOs);

                    }
                }
                
            }
            return _Myself;
        }

        public static RecongitionCollection getInstance()
        {
            if (_Myself == null || _Myself.RecongitionProcessors == null || _Myself.ResultProcessor == null)
            {
                throw new SoftwareException("Please use getInstance(List<IRecongitionProcessor> recongitionProcessors, RecongitionResultProcessor recongitionResultProcessor)");
            }
            return _Myself;
        }


    }
}
