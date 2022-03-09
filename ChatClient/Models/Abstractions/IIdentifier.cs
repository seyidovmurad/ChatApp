using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Models.Abstractions
{
    public interface IIdentifier
    {
        public string Id { get; set; }
    }
}
