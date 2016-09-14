﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Tracer
{
    public sealed class XmlTraceResultFormatter : ITraceResultFormatter
    {
        private Stream fOutStream;

        public XmlTraceResultFormatter(Stream outStream)
        {
            if (outStream == null)
            {
                throw new ArgumentNullException(nameof(outStream));
            }
            fOutStream = outStream;
        }

        public void Format(TraceResult traceResult)
        {
            if (traceResult == null)
            {
                throw new ArgumentNullException(nameof(traceResult));
            }

            var document = new XDocument();
            var root = new XElement("root");

            foreach (KeyValuePair<int, ThreadTraceInfo> threadTraceInfo in traceResult.ThreadsTraceInfo)
            {
                var threadElementInfo = new XElement("thread", new XAttribute("id", threadTraceInfo.Key));
                foreach (MethodTraceInfo methodTraceInfo in threadTraceInfo.Value.TracedMethods)
                {
                    threadElementInfo.Add(MethodTraceInfoToXElement(methodTraceInfo));
                }

                root.Add(threadElementInfo);
            }

            document.Add(root);
            document.Save(fOutStream);
        }

        // Static internals

        private static XElement MethodTraceInfoToXElement(MethodTraceInfo methodTraceInfo)
        {
            var result = new XElement("method");
            result.Add(new XAttribute("name", methodTraceInfo.Name));
            result.Add(new XAttribute("time", methodTraceInfo.ExecutionTime));
            result.Add(new XAttribute("class", methodTraceInfo.ClassName));

            foreach (MethodTraceInfo nestedMethodTraceInfo in methodTraceInfo.NestedCalls)
            {
                result.Add(MethodTraceInfoToXElement(nestedMethodTraceInfo));
            }

            return result;
        }
    }
}
