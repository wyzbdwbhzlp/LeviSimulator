using System;
using Utilities;

namespace DataCalculate
{
    namespace DataCalculate
    {
        public enum CalculateType
        {
            Add,
            Subtract,
        }

        public class CalculateHandler<T> where T : IConvertible
        {
            private CalculateType _calculateValueType;
            private T HandlerOperand;
            private string _handlerID;
            private int _belongCalculationLayer;
            
            
            public string HandlerID => _handlerID;
            public Type HandlerType=> typeof(T);
            public int BelongCalculationLayer => _belongCalculationLayer;

            public CalculateHandler(CalculateType calculateType, T operand,int belongCalculationLayer=0)
            {
                if (!IsValidOperand(operand))
                {
                    return;
                }
                _handlerID= Guid.NewGuid().ToString();
                HandlerOperand = operand;
                this._calculateValueType = calculateType;
                _belongCalculationLayer = belongCalculationLayer;
            }

            public T ExecuteCalculate(T inputValue)
            {
                if (!IsValidOperand(inputValue))
                {
                    return inputValue;
                }

                double input = Convert.ToDouble(inputValue);
                double operand = Convert.ToDouble(HandlerOperand);
                double result;

                switch (_calculateValueType)
                {
                    case CalculateType.Add:
                        result = input + operand;
                        break;
                    case CalculateType.Subtract:
                        result = input - operand;
                        break;
                    default:
                        LogUtil.LogError($"未被支持的计算类型: {_calculateValueType}");
                        throw new NotSupportedException($"Unsupported calculate value type: {_calculateValueType}");
                }

                return (T)Convert.ChangeType(result, typeof(T));
            }
            private bool IsValidOperand(T operand)
            {
                if (operand == null)
                {
                    LogUtil.LogError("操作数不能为null");
                    return false;
                }
                if (!(typeof(T) == typeof(int) || typeof(T) == typeof(float) || typeof(T) == typeof(double)))
                {
                    LogUtil.LogError($"不支持对 {typeof(T)} 进行操作");
                    throw new NotSupportedException($"Unsupported type: {typeof(T)}");
                }
                return true;
            }
        }
   
    }
}