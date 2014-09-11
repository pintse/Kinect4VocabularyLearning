using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ryan.ObjectRecognition.Service;
using Ryan.ObjectRecognition.DAO;

namespace Ryan.ObjectRecognition.Factory
{
    /// <summary>
    /// 物件辨識模組產生服務物件之工廠
    /// </summary>
    class ServiceFactory 
    {
        private static volatile ServiceFactory _Myself;
        private static readonly object ticket = new object();
        private static DAOFactory _DAOFactory;


        IRecongitionProcessor _SURFRecongitionProcessor, _ColorRecongitionProcessor;


        private ServiceFactory(DAOFactory daoOFactory)
        {
            _DAOFactory = daoOFactory;
        }

        public static ServiceFactory getInstance(DAOFactory daoOFactory)
        {
            if (_Myself == null)
            {
                lock (ticket)
                {
                    if (_Myself == null)
                    {
                        _Myself = new ServiceFactory(daoOFactory);
                    }
                }
                
            }

            return _Myself;
        }

        public IRecongitionProcessor getSURFRecongitionProcessorInstance()
        {
            if (_SURFRecongitionProcessor == null)
            {
                _SURFRecongitionProcessor = SURFRecongitionProcessor.getInstance(_DAOFactory.getObjectSURFDAOInstance());
            }
            return _SURFRecongitionProcessor;
        }

        public IRecongitionProcessor getColorRecongitionProcessorInstance()
        {
            if (_ColorRecongitionProcessor == null)
            {
                _ColorRecongitionProcessor = ColorRecongitionProcessor.getInstance( ClassifiedColor.getInstance() , _DAOFactory.getObjectColorDAOInstance());
            }
            return _ColorRecongitionProcessor;
        }

        public List<IRecongitionProcessor> getRecongitionProcessors()
        {
            List<IRecongitionProcessor> recognitionProcessors = new List<IRecongitionProcessor>();
            recognitionProcessors.Add(getColorRecongitionProcessorInstance());
            recognitionProcessors.Add(getSURFRecongitionProcessorInstance());

            return recognitionProcessors;
        }

        public IRecongitionResultProcessor getRecongitionResultProcessor()
        {
            return RecongitionResultProcessor.getInstance();
        }

        public RecongitionCollection getRecongitionCollection()
        {
            List<IObjectFeatureDAO> objectFeatureDAOs = new List<IObjectFeatureDAO>();
            objectFeatureDAOs.Add((IObjectFeatureDAO)_DAOFactory.getObjectColorDAOInstance());
            objectFeatureDAOs.Add((IObjectFeatureDAO)_DAOFactory.getObjectSURFDAOInstance());
            return getRecongitionCollection(getRecongitionProcessors(), getRecongitionResultProcessor(), objectFeatureDAOs);
        }

        public RecongitionCollection getRecongitionCollection(List<IRecongitionProcessor> recongitionProcessors, IRecongitionResultProcessor recongitionResultProcessor, List<IObjectFeatureDAO> objectFeatureDAOs)
        {
            return RecongitionCollection.getInstance(recongitionProcessors, recongitionResultProcessor, objectFeatureDAOs);
        }

        public RecongitionHandler getRecongitionHandler()
        {
            return getRecongitionHandler(getRecongitionCollection());
        }

        public RecongitionHandler getRecongitionHandler(RecongitionCollection recongitionCollection)
        {
            return RecongitionHandler.getInstance(recongitionCollection);
        }

        public GlobalDataService getGlobalDataService()
        {
            return GlobalDataService.getInstance();
        }
          
    }
}
