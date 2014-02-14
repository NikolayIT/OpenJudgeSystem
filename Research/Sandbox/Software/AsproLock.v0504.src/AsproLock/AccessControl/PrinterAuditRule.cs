using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>
    /// 
    /// </summary>
    public class PrinterAuditRule : AuditRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrinterAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="serviceRights">The service rights.</param>
        /// <param name="type">The type.</param>
        public PrinterAuditRule(IdentityReference identity, PrinterRights serviceRights, AuditFlags type)
            : base(identity, (int)serviceRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrinterAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="serviceRights">The service rights.</param>
        /// <param name="type">The type.</param>
        public PrinterAuditRule(string identity, PrinterRights serviceRights, AuditFlags type)
            : this(new NTAccount(identity), serviceRights, type)
        {
        }

        /// <summary>
        /// Gets the printer rights associated with this rule.
        /// </summary>
        /// <value>The combination of <see cref="PrinterRights"/> values that defines
        /// the rights associated with this rule.</value>
        public PrinterRights PrinterRights
        {
            get
            {
                return (PrinterRights)base.AccessMask;
            }
        }
    }
}
