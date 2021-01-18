using System;
using System.Collections.Generic;
using System.Text;

namespace AbandonOldPullRequests
{
    public class PullRequestArray
    {
        public PullRequest[] value { get; set; }
    }

    public class Value
    {
        PullRequest pullRequest { get; set; }
    }

}
