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

        private AOSInterviewer _aosInterviewer = new AOSInterviewer();
        private ASeeInterviewer _aseeInterviewer = new ASeeInterviewer();
        private FilminInterviewer _filminInterviewer = new FilminInterviewer();
        private TVBestInterviewer _tvbestInterviewer = new TVBestInterviewer();

        private Thread _aosThread;
        private Thread _aseeThread;
        private Thread _filminThread;
        private Thread _tvbestThread;

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
        /// <param name="query">A string for server to find</param>
        public virtual void ConductInterview(string query)
        {
            FormThreadsForOneQuery();

            _aosThread.Start(query);
            _aseeThread.Start(query);
            _filminThread.Start(query);
            _tvbestThread.Start(query);

            _aosThread.Join();
            _aseeThread.Join();
            _filminThread.Join();
            _tvbestThread.Join();

            AggregateResults();
        }

        /// <summary>
        /// Fill InterviewResult DataTable with concern search results
        /// </summary>
        /// <param name="query">Strings for server to find</param>
        public virtual void ConductInterview(string[] query)
        {
            FormThreadsForMultipleQueries();

            _aosThread.Start(query);
            _aseeThread.Start(query);
            _filminThread.Start(query);
            _tvbestThread.Start(query);

            _aosThread.Join();
            _aseeThread.Join();
            _filminThread.Join();
            _tvbestThread.Join();
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

        private void FormThreadsForOneQuery()
        {
            _aosThread = new Thread((object query) =>
            {
                _aosInterviewer.ConductInterview((string)query);
                MessageBox.Show("AOS Results Number: " + _aosInterviewer.InterviewResult.Rows.Count.ToString());
            });
            _aseeThread = new Thread((object query) =>
            {
                _aseeInterviewer.ConductInterview((string)query);
                MessageBox.Show("ASee Results Number: " + _aseeInterviewer.InterviewResult.Rows.Count.ToString());
            });
            _filminThread = new Thread((object query) =>
            {
                _filminInterviewer.ConductInterview((string)query);
                MessageBox.Show("Filmin Results Number: " + _filminInterviewer.InterviewResult.Rows.Count.ToString());
            });
            _tvbestThread = new Thread((object query) =>
            {
                _tvbestInterviewer.ConductInterview((string)query);
                MessageBox.Show("TVBest Results Number: " + _tvbestInterviewer.InterviewResult.Rows.Count.ToString());
            });
        }

        private void FormThreadsForMultipleQueries()
        {
            _aosThread = new Thread((object query) =>
            {
                _aosInterviewer.ConductInterview((string[])query);
                MessageBox.Show("AOS Results Number: " + _aosInterviewer.InterviewResult.Rows.Count.ToString());
            });
            _aseeThread = new Thread((object query) =>
            {
                _aseeInterviewer.ConductInterview((string[])query);
                MessageBox.Show("ASee Results Number: " + _aseeInterviewer.InterviewResult.Rows.Count.ToString());
            });
            _filminThread = new Thread((object query) =>
            {
                _filminInterviewer.ConductInterview((string[])query);
                MessageBox.Show("Filmin Results Number: " + _filminInterviewer.InterviewResult.Rows.Count.ToString());
            });
            _tvbestThread = new Thread((object query) =>
            {
                _tvbestInterviewer.ConductInterview((string[])query);
                MessageBox.Show("TVBest Results Number: " + _tvbestInterviewer.InterviewResult.Rows.Count.ToString());
            });
        }

        private void AggregateResults()
        {
            throw new NotImplementedException();
        }

        #endregion (Methods)
    }
}
