﻿using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmAdo.Operations
{
    public class SelectMultipleEntitiesOperation : CrmOperation
    {
        public SelectMultipleEntitiesOperation(List<ColumnMetadata> columnMetadata, OrganizationRequest request)
        {
            this.Columns = columnMetadata;
            this.Request = request;
        }

        protected override OrganisationRequestCommandResult ExecuteCommand()
        {
            OrganisationRequestCommandResult commandResponse = null;
            var resultSet = CreateEntityResultSet();
            if (IsSchemaOnly())
            {
                commandResponse = new OrganisationRequestCommandResult(null, resultSet, false);
            }
            else
            {
                var response = ExecuteOrganisationRequest();
                commandResponse = new OrganisationRequestCommandResult(response, resultSet, false);
                var retrieveMultipleResponse = response as RetrieveMultipleResponse;
                if (retrieveMultipleResponse != null)
                {
                    resultSet.Results = retrieveMultipleResponse.EntityCollection;
                }
            }

            return commandResponse;
        }
    }
}
