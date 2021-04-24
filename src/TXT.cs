using D2SLib.Model.TXT;
using System;
using System.Collections.Generic;
using System.Text;

namespace D2SLib
{
    public class TXT
    {
        public ItemStatCostTXT ItemStatCostTXT { get; set; }
        private ItemsTXT _ItemsTXT = null;
        public ItemsTXT ItemsTXT
        {
            get
            {
                if(_ItemsTXT == null)
                {
                    _ItemsTXT = new ItemsTXT();
                }
                return _ItemsTXT;
            }
            set
            {
                _ItemsTXT = value;
            }
        }
    }
}
