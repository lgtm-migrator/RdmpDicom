﻿using Rdmp.Core.Caching.Pipeline.Sources;
using Rdmp.Core.Caching.Requests;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using System;
using System.Diagnostics;

namespace Rdmp.Dicom.Cache.Pipeline
{
    public class ProcessBasedCacheSource : CacheSource<SMIDataChunk>
    {
        [DemandsInitialization(@"Process to start.  Template with 
%s start time
%e end time time to fetch
%d directory to put files fetched
Example:. './GetImages.exe ""%s"" ""%e%""'",Mandatory = true)]
        public string Command {get;set;}

        [DemandsInitialization("The datetime format for %s and %e.",Mandatory = true,DefaultValue = "yyyy-MM-dd HH:mm:ss")]
        public string TimeFormat {get;set;}

        [DemandsInitialization("True to throw an Exception if the process run returns a nonzero exit code", DefaultValue = true)]
        public bool ThrowOnNonZeroExitCode {get;set;}

        public override void Abort(IDataLoadEventListener listener)
        {
            
        }

        public override void Check(ICheckNotifier notifier)
        {
            
        }

        public override void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            
        }

        public override SMIDataChunk DoGetChunk(ICacheFetchRequest request, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {                     
            listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information,$"ProcessBasedCacheSource version is {typeof(ProcessBasedCacheSource).Assembly.GetName().Version}.  Assembly is {typeof(ProcessBasedCacheSource).Assembly} " ));
            
            // Where we are putting the files
            var cacheDir = new LoadDirectory(Request.CacheProgress.LoadProgress.LoadMetadata.LocationOfFlatFiles).Cache;
            var cacheLayout = new SMICacheLayout(cacheDir, new SMICachePathResolver("ALL"));
            
            Chunk = new SMIDataChunk(Request)
            {
                FetchDate = Request.Start,
                Modality = "ALL",
                Layout = cacheLayout
            };
            
            var workingDirectory = cacheLayout.GetLoadCacheDirectory(listener);

            listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information,"Working directory is:" + workingDirectory));
            listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information,"Fetch Start is:" + request.Start));
            listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information,"Fetch End is:" + request.End));

            listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information,"Command template is:" + Command));
            listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information,"Datetime format is:" + TimeFormat));
            

            string toRun = Command
                .Replace("%s",request.Start.ToString(TimeFormat))
                .Replace("%e",request.End.ToString(TimeFormat))
                .Replace("%d",workingDirectory.FullName);

            listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information,"Running Process:" + toRun));
              
            var p = Process.Start(toRun);
            p.WaitForExit();

            listener.OnNotify(this,new NotifyEventArgs( p.ExitCode == 0 ? ProgressEventType.Information : ProgressEventType.Warning , "Process exited with code " + p.ExitCode));

            if(p.ExitCode != 0 && ThrowOnNonZeroExitCode)
                throw new Exception("Process exited with code " + p.ExitCode);
            
            return Chunk;
        }

        public override SMIDataChunk TryGetPreview()
        {
            return null;
        }
    }
}
