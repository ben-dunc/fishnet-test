using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Edgegap.Editor.Api.Models.Results
{
    [Serializable]
    public class ListAllDeploymentsResult
    {
        public List<Datum> data;
        public int total_count;
        public Pagination pagination;
    }

    [Serializable]
    public class Datum
    {
        public string request_id;
        public string fqdn;
        public string start_time;
        public bool ready;
        public string public_ip;
        public Ports ports;
        public List<string> tags;
        public string sockets;
        public string sockets_usage;
        public bool is_joinable_by_session;
    }

    [Serializable]
    public class Pagination
    {
        public int number;
        public int next_page_number;
        public int previous_page_number;
        public Paginator paginator;
        public bool has_next;
        public bool has_previous;
    }

    [Serializable]
    public class Paginator
    {
        public int num_pages;
    }

    [Serializable]
    public class Ports
    {
        [JsonProperty("Game Port")]
        public Port GamePort;
    }

    [Serializable]
    public class Port
    {
        public int external = -1;
        [JsonProperty("internal")]
        public int Internal = -1;
        public string protocol;
        public string name;
        public bool tls_upgrade;
        public string link;
        public int proxy;
    }
}
