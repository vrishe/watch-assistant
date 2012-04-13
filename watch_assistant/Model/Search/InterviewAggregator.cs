using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.ComponentModel;
using System.Windows;
using System.Threading.Tasks;
using System.Threading;

namespace watch_assistant.Model.Search
{
    class InterviewAggregator : IInterviewer
    {
        #region Fields

        private DataTable _interviewResult;

        //private AOSInterviewer _aosInterviewer = new AOSInterviewer();
        //private ASeeInterviewer _aseeInterviewer = new ASeeInterviewer();
        //private FilminInterviewer _filminInterviewer = new FilminInterviewer();
        //private TVBestInterviewer _tvbestInterviewer = new TVBestInterviewer();

        //private Thread _aosThread;
        //private Thread _aseeThread;
        //private Thread _filminThread;
        //private Thread _tvbestThread;

        private readonly Dictionary<InterviewerBase, Thread> _interviewers = new Dictionary<InterviewerBase, Thread>();

        #endregion (Fields)

        #region IInterviewer implementation

        /// <summary>
        /// Gets DataTable with concern search results 
        /// </summary>
        public DataTable InterviewResult
        {
            get
            {
                if (_interviewResult != null)
                    return _interviewResult;
                _interviewResult = new DataTable("InterviewResults");
                return _interviewResult;
            }
        }

        /// <summary>
        /// Fill InterviewResult DataTable with concern search results
        /// </summary>
        /// <param name="query">Strings for server to find</param>
        public virtual void ConductInterview(string[] query)
        {
            FormInterviewers();

            foreach (Thread t in _interviewers.Values) t.Start(query);
            foreach (Thread t in _interviewers.Values) 
                if (t.IsAlive)
                    t.Join();

            AggregateResults();
        }

        /// <summary>
        /// Clear past interviews results
        /// </summary>
        public void ClearInterviewResults()
        {
            _interviewResult = null;
        }

        #endregion (IInterviewer implementation)

        #region Methods

        private void FormInterviewers()
        {
            InterviewerBase aos;
            _interviewers.Add(aos = new AOSInterviewer(), new Thread((object query) =>
            {
                aos.ConductInterview((string[])query);
                //MessageBox.Show("AOS Results Number: " + aos.InterviewResult.Rows.Count.ToString());
            }));
            InterviewerBase asee;
            _interviewers.Add(asee = new ASeeInterviewer(), new Thread((object query) =>
            {
                asee.ConductInterview((string[])query);
                //MessageBox.Show("ASee Results Number: " + asee.InterviewResult.Rows.Count.ToString());
            }));
            InterviewerBase tvbest;
            _interviewers.Add(tvbest = new TVBestInterviewer(), new Thread((object query) =>
            {
                tvbest.ConductInterview((string[])query);
                //MessageBox.Show("TVBest Results Number: " + tvbest.InterviewResult.Rows.Count.ToString());
            }));
            InterviewerBase filmin;
            _interviewers.Add(filmin = new FilminInterviewer(), new Thread((object query) =>
            {
                filmin.ConductInterview((string[])query);
                //MessageBox.Show("Filmin Results Number: " + filmin.InterviewResult.Rows.Count.ToString());
            }));
        }

        private void AggregateResults()
        {

            foreach (InterviewerBase i in _interviewers.Keys)
                if (_interviewResult == null)
                    _interviewResult = i.InterviewResult.Copy();
                else
                    foreach (DataRow row in i.InterviewResult.Rows)
                        _interviewResult.ImportRow(row);                    
        }

        #endregion (Methods)
    }
}
