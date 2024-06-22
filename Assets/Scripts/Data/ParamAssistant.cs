using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BackEnd
{
    public enum Data
    {
        TotalGold, KillCount, PlayTime
    }
    public class ColumnName
    {
        public const string TotalGoldKey = "Gold";
        public const string KillCountKey = "Kill";
        public const string PlayTimeKey = "Time";

        public string this[Data data]
        {
            get
            {
                switch (data)
                {
                    case Data.TotalGold: return TotalGoldKey;
                    case Data.KillCount: return KillCountKey;
                    case Data.PlayTime: return PlayTimeKey;

                    default:
                        return string.Empty;
                }
            }
        }
    }
    public class ParamAssistant : Singleton<ParamAssistant>
    {
        BackEndServerManager backEndServerManager = new BackEndServerManager();

        private ColumnName _Column = new ColumnName();

        public Param GetData(params Data[] data)
        {
            Param param = new Param();

            for (int i = 0; i < data.Length; i++)
            {
                switch (data[i])
                {
                    case Data.TotalGold:
                        param.Add(_Column[data[i]], MoneyManager.Instance.Money);
                        break;
                    case Data.KillCount:
                        param.Add(_Column[data[i]], GameLoger.Instance.KillCount);
                        break;
                    case Data.PlayTime:
                        param.Add(_Column[data[i]], GameLoger.Instance.ElapsedTime);
                        break;
                }
            }
            return param;
        }


    }

}