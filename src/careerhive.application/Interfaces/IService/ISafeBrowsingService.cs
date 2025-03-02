using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace careerhive.application.Interfaces.IService;
public interface ISafeBrowsingService
{
    Task<bool> IsUrlSafeAsync(string url);
}
