/*
 * Copyright 2010 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Net;
using Gtk;
using Common;
using Common.Gtk;
using Common.Utils;
using ZeroInstall.Injector;

public partial class MainWindow : Window, IHandler
{
    #region Events    
    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }       
    #endregion
    
    #region Constructor
    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();
    }
    #endregion

    //--------------------//

    #region Async control
    /// <summary>
    /// Starts the GUI in a separate thread.
    /// </summary>
    public void ShowAsync()
    {
        // ToDo
    }

    /// <summary>
    /// Begins closing the window running in the GUI thread. Does not wait for the window to finish closing.
    /// </summary>
    public void CloseAsync()
    {
        // ToDo
    }
    #endregion

    #region Handler
    /// <summary>
    /// Silently answer all questions with "No".
    /// </summary>
    public bool Batch { get; set; }

    /// <inheritdoc />
    public bool AcceptNewKey(string information)
    {
        if (Batch) return false;

        return GtkMsg.Ask(this, information, MsgSeverity.Info);
    }

    /// <inheritdoc />
    public void RunDownloadTask(ITask task)
    {
        //labelOperation.Text = task.Name + @"...";
        //progressBar.Task = task;
        task.RunSync();
    }

    /// <inheritdoc />
    public void RunIOTask(ITask task)
    {
        //labelOperation.Text = task.Name + @"...";
        //progressBar.Task = task;
        task.RunSync();
    }
    #endregion
}
