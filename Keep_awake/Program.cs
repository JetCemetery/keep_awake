using System;
using System.Threading;
 
namespace Keep_Awake
{

    class Program
    {
        private static Win32.POINT beforePOS;
        private static Win32.POINT afterPOS;
        private static int timeout = 15000; //15000;
        private static Win32.EXECUTION_STATE original_EXE_state;// = null;
        private static bool prevent_sleep_Enabled;
        static void Main(string[] args)
        {
            beforePOS = Win32.GetCursorPosition();
            afterPOS = Win32.GetCursorPosition();
            original_EXE_state = GetOriginal_EXE_state();
            prevent_sleep_Enabled = false;
            Thread t = new Thread(watchdog);
            t.Start();
        }
 
        private static void watchdog()
        {
            DateTime today = DateTime.Today;
            DateTime stop_process = new DateTime(today.Year, today.Month, today.Day, 18, 0, 0);
 
            while (true)
            {
                beforePOS = Win32.GetCursorPosition();
                Thread.Sleep(timeout);
                afterPOS = Win32.GetCursorPosition();
                if (DidMouseMove() == false)
                {
                    //System.Diagnostics.Debug.WriteLine("moving mouse");
                    PreventSleep();
                    MoveMouse();
                }
                else { reenabledSleep(); }
                   
                if(DateTime.Now >= stop_process)
                    break;
            }
            reenabledSleep();
        }

        private static void MoveMouse()
        {
            Win32.POINT curPOS = Win32.GetCursorPosition();
            curPOS.y = curPOS.y <= 100 ? curPOS.y += 1 : curPOS.y + -1;
            curPOS.x = curPOS.x <= 100 ? curPOS.x += 1 : curPOS.x + -1;
            Win32.SetCursorPos(curPOS.x, curPOS.y);
        }
 
        private static bool DidMouseMove()
        {
            return (beforePOS.x != afterPOS.x || beforePOS.y != afterPOS.y) ? true : false;
        }
 
        private static Win32.EXECUTION_STATE GetOriginal_EXE_state()
        {
            return Win32.SetThreadExecutionState(Win32.EXECUTION_STATE.ES_CONTINUOUS);
        }
 
        private static void reenabledSleep()
        {
            if (prevent_sleep_Enabled)
            {
                Win32.SetThreadExecutionState(original_EXE_state);
                prevent_sleep_Enabled = false;
            }  
        }
 
        private static void PreventSleep()
        {
            if (prevent_sleep_Enabled == false)
            {
                Win32.SetThreadExecutionState(
                    Win32.EXECUTION_STATE.ES_DISPLAY_REQUIRED |
                    Win32.EXECUTION_STATE.ES_CONTINUOUS |
                    Win32.EXECUTION_STATE.ES_AWAYMODE_REQUIRED);
                prevent_sleep_Enabled = true;
            }
 
        }
    }
}

