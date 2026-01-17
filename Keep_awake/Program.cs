using System.Diagnostics;
 
namespace Keep_Awake
{

    class Program
    {
        #region Compile options
        private static bool RunForver = false;
        #endregion

        #region Needed local variables for operation
        private static Win32.POINT beforePOS;
        private static Win32.POINT afterPOS;
        private static int timeout = 15000; //15000;
        private static Win32.EXECUTION_STATE InitialPowerState;// = null;
        private static bool SleepDisabled;
        #endregion
        static void Main(string[] args)
        {
            if (OnlyOneInstance())
            {
                beforePOS = Win32.GetCursorPosition();
                afterPOS = Win32.GetCursorPosition();
                InitialPowerState = GetCurrentPowerState();
                SleepDisabled = false;
                Thread t = new Thread(Watchdog);
                t.Start();
            }
        }

        private static void Watchdog()
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
                    PreventSleep();
                    MoveMouse();
                }
                else { ReenabledSleep(); }
                   
                if(RunForver == false)
                {
                    if (DateTime.Now >= stop_process)
                        break;
                }

            }
            ReenabledSleep();
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
 
        private static Win32.EXECUTION_STATE GetCurrentPowerState()
        {
            return Win32.SetThreadExecutionState(Win32.EXECUTION_STATE.ES_CONTINUOUS);
        }
 
        private static void ReenabledSleep()
        {
            if (SleepDisabled)
            {
                Win32.SetThreadExecutionState(InitialPowerState);
                SleepDisabled = false;
            }  
        }
 
        private static void PreventSleep()
        {
            if (SleepDisabled == false)
            {
                Win32.SetThreadExecutionState(
                    Win32.EXECUTION_STATE.ES_DISPLAY_REQUIRED |
                    Win32.EXECUTION_STATE.ES_CONTINUOUS |
                    Win32.EXECUTION_STATE.ES_AWAYMODE_REQUIRED);
                SleepDisabled = true;
            }
 
        }

        private static bool OnlyOneInstance()
        {
            //Prevent this specific program to run mutiple times
            //if tool detects two (or more) instance like this running, you shall return false
            bool returningBool = true;
            short totalHits = 0;
            string? fullpath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            if (string.IsNullOrEmpty(fullpath) == false)
            {
                string fname = Path.GetFileNameWithoutExtension(fullpath);
                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(process.ProcessName) == false)
                        {
                            if (process.ProcessName.Equals(fname))
                            {
                                string? curFpath = process.MainModule?.FileName;
                                if (string.IsNullOrEmpty(curFpath) == false)
                                {
                                    if (curFpath.Equals(fullpath))
                                    {
                                        totalHits++;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            //if you have more than one, user is double tapping.
            if (totalHits > 1)
                returningBool = false;
            return returningBool;
        }
    }
}

