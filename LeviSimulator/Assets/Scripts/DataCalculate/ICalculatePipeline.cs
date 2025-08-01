using System;
using System.Collections.Generic;
using DataCalculate.DataCalculate;

namespace DataCalculate
{
    public interface ICalculatePipeline
    {
        string AddHandler(object handler);
        void RemoveHandler(object handler);
        object ExecuteCalculate(object initialValue);
        void InitPipeline(Type valueType);
        Type ValueType { get; }
        void ClearPipeline();
        string[] GetHandlerIDs();
    }
}