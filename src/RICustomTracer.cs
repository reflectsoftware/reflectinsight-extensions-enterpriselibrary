// ***********************************************************************
// Assembly         : ReflectSoftware.Insight.Extensions.EnterpriseLibrary
// Author           : ReflectSoftware Inc.
// Created          : 03-19-2014
// Last Modified On : 03-28-2014
// ***********************************************************************
// <copyright file="RICustomTracer.cs" company="ReflectSoftware, Inc.">
//     Copyright (c) ReflectSoftware, Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

using ReflectSoftware.Insight.Common;

/// <summary>
/// Namespace - Tracer
/// </summary>
namespace ReflectSoftware.Insight.Extensions.EnterpriseLibrary.Tracer
{
    //-------------------------------------------------------------------------
    /// <summary>   RICustomTracer Class. </summary>    
    /// <seealso cref="T:Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners.CustomTraceListener"/>
    //-------------------------------------------------------------------------
    [ConfigurationElementType(typeof(CustomTraceListenerData))]
    public class RICustomTracer : CustomTraceListener
    {
        /// <summary>   The name. </summary>
        protected String _Name;
        /// <summary>   The trace listener. </summary>
        protected RITraceListener FRITraceListener;

        //---------------------------------------------------------------------        
        /// <summary>   Initializes a new instance of the <see cref="RICustomTracer" /> class. </summary>
        /// <remarks>   The default name of 'RICustomTracer' will be used. </remarks>
        //---------------------------------------------------------------------
        public RICustomTracer()
        {
            FRITraceListener = null;
            Name = "RICustomTracer";
        }
        //---------------------------------------------------------------------
        /// <summary>   Disposes the listener. </summary>
        //---------------------------------------------------------------------
        protected void DisposeListener()
        {
            lock (this)
            {
                if (FRITraceListener != null)
                {
                    FRITraceListener.Dispose();
                    FRITraceListener = null;
                }
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Releases the unmanaged resources used by the
        /// <see cref="T:System.Diagnostics.TraceListener" /> and optionally releases the managed
        /// resources.
        /// </summary>
        ///
        /// <seealso cref="M:System.Diagnostics.TraceListener.Dispose(bool)"/>
        /// ### <param name="disposing">    true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>        
        //---------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            DisposeListener();
            base.Dispose(disposing);
        }
        //---------------------------------------------------------------------        
        /// <summary>
        /// When overridden in a derived class, closes the output stream so it no longer receives tracing
        /// or debugging output.
        /// </summary>
        ///
        /// <remarks>   ReflectInsight Version 5.3. </remarks>
        ///
        /// <seealso cref="M:System.Diagnostics.TraceListener.Close()"/>
        //---------------------------------------------------------------------
        public override void Close()
        {
            DisposeListener();
            base.Close();
        }
        //---------------------------------------------------------------------
        /// <summary>   Gets the listener. </summary>
        ///
        /// <value> The <see cref="ReflectSoftware.Insight.TraceListener"/> </value>
        //---------------------------------------------------------------------
        protected RITraceListener Listener
        {
            get
            {
                lock (this)
                {
                    if (FRITraceListener == null)
                    {
                        FRITraceListener = new RITraceListener(Attributes["Extension"] ?? "RITraceListener");
                    }
                }
                    
                return FRITraceListener;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Writes trace information, a data object and event information to the listener specific output.
        /// </summary>
        ///
        /// <seealso cref="M:System.Diagnostics.TraceListener.TraceData(TraceEventCache,String,TraceEventType,Int32,Object)"/>
        /// ### <param name="eventCache">   A <see cref="T:System.Diagnostics.TraceEventCache" /> object  that contains the current process ID, thread ID, and stack trace information. </param>
        /// ### <param name="source">       A name used to identify the output, typically the name of the application that generated the trace event. </param>
        /// ### <param name="eventType">    One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace. </param>
        /// ### <param name="id">           A numeric identifier for the event. </param>
        /// ### <param name="data">         The trace data to emit. </param>
        //---------------------------------------------------------------------
        public override void TraceData(TraceEventCache eventCache, String source, TraceEventType eventType, Int32 id, Object data)
        {
            if (data is XmlLogEntry)
            {
                RITraceListenerData tData = new RITraceListenerData();
                tData.MessageType = MessageType.SendXML;
                tData.Message = (data as XmlLogEntry).Message;
                tData.Details = (data as XmlLogEntry).Xml.InnerXml;
                tData.ExtendedProperties = (data as XmlLogEntry).ExtendedProperties;
                
                Listener.TraceData(eventCache, source, eventType, id, tData);
            }
            else if (data is LogEntry)
            {
                RITraceListenerData tData = new RITraceListenerData() { Message = (data as LogEntry).Message };
                RITraceListener.PrepareListenerData(tData, eventType);
                tData.ExtendedProperties = (data as LogEntry).ExtendedProperties;

                Listener.TraceData(eventCache, source, eventType, id, tData);
            }
            else if (data is string)
            {
                Write(data as string);
            }
            else
            {
                base.TraceData(eventCache, source, eventType, id, data);
            }
        }
        //---------------------------------------------------------------------        
        /// <summary>
        /// Writes trace information, a message, and event information to the listener specific output.
        /// </summary>
        ///
        /// <seealso cref="M:System.Diagnostics.TraceListener.TraceEvent(TraceEventCache,string,TraceEventType,int,string)"/>
        /// ### <param name="eventCache">   A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information. </param>
        /// ### <param name="source">       A name used to identify the output, typically the name of the application that generated the trace event. </param>
        /// ### <param name="eventType">    One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace. </param>
        /// ### <param name="id">           A numeric identifier for the event. </param>
        /// ### <param name="message">      A message to write. </param>
        //---------------------------------------------------------------------
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            Listener.TraceEvent(eventCache, source, eventType, id, message);
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Writes trace information, a formatted array of objects and event information to the listener
        /// specific output.
        /// </summary>
        ///
        /// <seealso cref="M:System.Diagnostics.TraceListener.TraceEvent(TraceEventCache,string,TraceEventType,int,string,params object[])"/>
        /// ### <param name="eventCache">   A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information. </param>
        /// ### <param name="source">       A name used to identify the output, typically the name of the application that generated the trace event. </param>
        /// ### <param name="eventType">    One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace. </param>
        /// ### <param name="id">           A numeric identifier for the event. </param>
        /// ### <param name="format">       A format string that contains zero or more format items, which correspond to objects in the <paramref name="args" /> array. </param>
        /// ### <param name="args">         An object array containing zero or more objects to format. </param>
        //---------------------------------------------------------------------
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            Listener.TraceEvent(eventCache, source, eventType, id, format, args);
        }
        //---------------------------------------------------------------------
        /// <summary>   Fails with the specified message. </summary>
        ///
        /// <seealso cref="M:System.Diagnostics.TraceListener.Fail(String)"/>
        /// ### <param name="message">  A message to emit. </param>
        //---------------------------------------------------------------------
        public override void Fail(String msg)
        {
            Listener.Fail(msg);
        }
        //---------------------------------------------------------------------
        /// <summary>   Fails with the specified and category. </summary>
        ///
        /// <seealso cref="M:System.Diagnostics.TraceListener.Fail(String,String)"/>
        /// ### <param name="message">          A message to emit. </param>
        /// ### <param name="detailMessage">    A detailed message to emit. </param>
        //---------------------------------------------------------------------
        public override void Fail(String msg, String category)
        {
            Listener.Fail(msg, category);
        }

        //---------------------------------------------------------------------
        /// <summary>   Writes the specified message. </summary>
        ///
        /// <seealso cref="M:System.Diagnostics.TraceListener.Write(String)"/>
        /// ### <param name="message">  A message to write. </param>
        //---------------------------------------------------------------------
        public override void Write(String msg)
        {
            Listener.Write(msg);
        }

        //---------------------------------------------------------------------
        /// <summary>   Writes the specified object. </summary>
        ///
        /// <seealso cref="M:System.Diagnostics.TraceListener.Write(Object)"/>
        /// ### <param name="o">    An <see cref="T:System.Object" /> whose fully qualified class name you want to write. </param>
        //---------------------------------------------------------------------
        public override void Write(Object obj)
        {
            Listener.Write(obj);
        }

        //---------------------------------------------------------------------
        /// <summary>   Writes the specified message and category. </summary>
        ///
        /// <remarks>   ReflectInsight Version 5.3. </remarks>
        ///
        /// <seealso cref="M:System.Diagnostics.TraceListener.Write(String,String)"/>
        /// ### <param name="message">  A message to write. </param>
        /// ### <param name="category"> A category name used to organize the output. </param>
        //---------------------------------------------------------------------
        public override void Write(String msg, String category)
        {
            Listener.Write(msg, category);
        }

        //---------------------------------------------------------------------
        /// <summary>   Writes the line using aspecific object and category. </summary>
        ///
        /// <seealso cref="M:System.Diagnostics.TraceListener.Write(Object,String)"/>
        /// ### <param name="o">        An <see cref="T:System.Object" /> whose fully qualified class name you want to write. </param>
        /// ### <param name="category"> A category name used to organize the output. </param>
        //---------------------------------------------------------------------
        public override void Write(Object obj, String category)
        {
            Listener.Write(obj, category);
        }

        //---------------------------------------------------------------------
        /// <summary>   Writes the line using a specific message. </summary>
        ///
        /// <seealso cref="M:System.Diagnostics.TraceListener.WriteLine(String)"/>
        /// ### <param name="message">  A message to write. </param>
        //---------------------------------------------------------------------
        public override void WriteLine(String msg)
        {
            Listener.WriteLine(msg);
        }

        //---------------------------------------------------------------------
        /// <summary>   Writes the line using a specific object. </summary>
        ///
        /// <seealso cref="M:System.Diagnostics.TraceListener.WriteLine(Object)"/>
        /// ### <param name="o">    An <see cref="T:System.Object" /> whose fully qualified class name you want to write. </param>
        //---------------------------------------------------------------------
        public override void WriteLine(Object obj)
        {
            Listener.WriteLine(obj);
        }

        //---------------------------------------------------------------------
        /// <summary>   Writes the line using a specific message and category. </summary>
        ///
        /// <seealso cref="M:System.Diagnostics.TraceListener.WriteLine(String,String)"/>
        /// ### <param name="message">  A message to write. </param>
        /// ### <param name="category"> A category name used to organize the output. </param>
        //---------------------------------------------------------------------
        public override void WriteLine(String msg, String category)
        {
            Listener.WriteLine(msg, category);
        }

        //---------------------------------------------------------------------
        /// <summary>   Writes the line using a specific object and category. </summary>
        ///
        /// <seealso cref="M:System.Diagnostics.TraceListener.WriteLine(Object,String)"/>
        /// ### <param name="o">        An <see cref="T:System.Object" /> whose fully qualified class name you want to write. </param>
        /// ### <param name="category"> A category name used to organize the output. </param>
        //---------------------------------------------------------------------
        public override void WriteLine(Object obj, String category)
        {
            Listener.WriteLine(obj, category);
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a name for this <see cref="T:System.Diagnostics.TraceListener" />
        /// </summary>
        ///
        /// <seealso cref="P:System.Diagnostics.TraceListener.Name"/>
        //---------------------------------------------------------------------
        public override String Name
        {
            get { return _Name; }            
            set { _Name = value; }            
        }

        //---------------------------------------------------------------------
        /// <summary>   Gets a value indicating whether the trace listener is thread safe. </summary>
        ///
        /// <seealso cref="P:System.Diagnostics.TraceListener.IsThreadSafe"/>
        //---------------------------------------------------------------------
        public override bool IsThreadSafe
        {
            get { return true; }
        }
    }    
}
