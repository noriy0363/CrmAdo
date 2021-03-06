﻿using CrmAdo.Dynamics;
using CrmAdo.Operations;
using CrmAdo.Visitor;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using SQLGeneration.Generators;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmAdo.Core
{
    public class SqlGenerationCrmOperationProvider : ICrmOperationProvider
    {
        //  private OrganizationRequestBuilderVisitor _BuilderVisitor;

        private IDynamicsAttributeTypeProvider _DynamicsAttributeTypeProvider;

        public const string ParameterToken = "@";

        public SqlGenerationCrmOperationProvider()
            : this(new DynamicsAttributeTypeProvider())
        {

        }

        public SqlGenerationCrmOperationProvider(IDynamicsAttributeTypeProvider dynamicsAttributeTypeProvider)
        {
            if (dynamicsAttributeTypeProvider == null)
            {
                throw new ArgumentNullException("dynamicsAttributeTypeProvider");
            }
            _DynamicsAttributeTypeProvider = dynamicsAttributeTypeProvider;
        }

        public ICrmOperation GetOperation(CrmDbCommand command, CommandBehavior behavior)
        {
            bool schemaOnly = (behavior & CommandBehavior.SchemaOnly) > 0;
            ICrmOperation result = null;

            switch (command.CommandType)
            {
                case CommandType.StoredProcedure:
                    throw new System.NotImplementedException();

                case CommandType.TableDirect:
                    result = GetOperationFromTableDirectCommand(command, behavior);
                    break;
                case CommandType.Text:
                    result = GetOperationFromTextCommand(command, behavior);
                    break;
                default:
                    throw new System.NotImplementedException();
            }
            return result;
        }

        private ICrmOperation GetOperationFromTextCommand(CrmDbCommand command, CommandBehavior behavior)
        {
            // We actually need to parse the SQL, and then build the appropriate organisation request.

            var commandText = command.CommandText;

            // Use SQLGeneration to parse the SQL command into a Visitable Builder.
            var commandBuilder = new CommandBuilder();
            var options = new CommandBuilderOptions();
            options.PlaceholderPrefix = ParameterToken;
            var sqlCommandBuilder = commandBuilder.GetCommand(commandText, options);

            // Visit the builder with out custom visiter that will build the appropriate org request whilst visiting.   
            var visitor = GetVisitor(command);
            if (visitor == null)
            {
                throw new InvalidOperationException("visitor was null");
            }
            sqlCommandBuilder.Accept(visitor);

            // The visitor should now have vuild the OrgCommand that we need.
            var orgCommand = visitor.CrmOperation;
            if (orgCommand == null || orgCommand.Request == null)
            {
                throw new NotSupportedException("Could not translate the command into the appropriate Organization Service Request Message");
            }

            // Before returning the command, ensure some additional properties are set.
            orgCommand.DbCommand = command;
            orgCommand.CommandBehavior = behavior;
            return orgCommand;

        }

        protected virtual OrganizationRequestBuilderVisitor GetVisitor(CrmDbCommand command)
        {
            var metaDataProvider = command.CrmDbConnection.MetadataProvider;
            var commandParams = command.Parameters;
            var settings = command.CrmDbConnection.Settings;
            return new OrganizationRequestBuilderVisitor(metaDataProvider, commandParams, _DynamicsAttributeTypeProvider, settings);
        }

        private ICrmOperation GetOperationFromTableDirectCommand(CrmDbCommand command, CommandBehavior behavior)
        {
            // No need to parse the command text as SQL because table direct commands should just contain the table name.
            // Therefore just construct a command that will execute a retreive multiple for the entity name specified.
            var request = GetRetrieveMultipleRequest(command, behavior);
            var result = new SelectMultipleEntitiesOperation(null, request);
            result.CommandBehavior = behavior;
            result.DbCommand = command;
            // result.OperationType = Enums.CrmOperation.RetrieveMultiple;
            return result;
        }

        private RetrieveMultipleRequest GetRetrieveMultipleRequest(CrmDbCommand command, CommandBehavior behavior)
        {
            var entityName = command.CommandText;
            if (entityName.Contains(" "))
            {
                throw new ArgumentException("When CommandType is TableDirect, CommandText should be the name of an entity.");
            }
            var request = new RetrieveMultipleRequest()
            {
                Query = new QueryExpression(entityName) { ColumnSet = new ColumnSet(true), PageInfo = new PagingInfo() { ReturnTotalRecordCount = true } }
            };
            return request;
        }

    }
}
