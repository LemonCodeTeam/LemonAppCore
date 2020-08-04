using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LemonAppCore.Helpers
{
    public class MyTimer
    {
        public int Interval { get; set; }
        public bool Enabled { get; private set; } = false;
        public event EventHandler Elapsed;
        public async void Start() {
            Enabled = true;
            while (Enabled) {
                await Task.Delay(Interval);
                Elapsed(null, null);
            }
        }
        public void Stop() {
            Enabled = false;
        }
    }
}
