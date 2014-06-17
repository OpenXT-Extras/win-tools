//
// Copyright (c) 2012 Citrix Systems, Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Udbus.WCF.Dbus.Contracts
{
    #region Surrogate Attribute
    public class DbusContainerAttribute : Attribute
        , System.ServiceModel.Description.IContractBehavior
        , System.ServiceModel.Description.IOperationBehavior
        , System.ServiceModel.Description.IWsdlExportExtension
    {

        #region IContractBehavior Members
        public void AddBindingParameters(System.ServiceModel.Description.ContractDescription contractDescription, System.ServiceModel.Description.ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyClientBehavior(System.ServiceModel.Description.ContractDescription contractDescription,
            System.ServiceModel.Description.ServiceEndpoint endpoint,
            System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
            foreach (System.ServiceModel.Description.OperationDescription opDesc in contractDescription.Operations)
            {
                ApplyDataContractSurrogate(opDesc);
            }
        }

        public void ApplyDispatchBehavior(System.ServiceModel.Description.ContractDescription contractDescription, System.ServiceModel.Description.ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime)
        {
            foreach (System.ServiceModel.Description.OperationDescription opDesc in contractDescription.Operations)
            {
                ApplyDataContractSurrogate(opDesc);
            }
        }

        public void Validate(System.ServiceModel.Description.ContractDescription contractDescription, System.ServiceModel.Description.ServiceEndpoint endpoint)
        {

        }
        #endregion // IContractBehavior Members

        #region IWsdlExportExtension Members
        public void ExportContract(System.ServiceModel.Description.WsdlExporter exporter, System.ServiceModel.Description.WsdlContractConversionContext context)
        {
            if (exporter == null)
            {
                throw new ArgumentNullException("exporter");
            }

            object dataContractExporter;
            XsdDataContractExporter xsdDCExporter;

            if (!exporter.State.TryGetValue(typeof(XsdDataContractExporter), out dataContractExporter))
            {
                xsdDCExporter = new XsdDataContractExporter(exporter.GeneratedXmlSchemas);
                exporter.State.Add(typeof(XsdDataContractExporter), xsdDCExporter);
            }
            else
            {
                xsdDCExporter = (XsdDataContractExporter)dataContractExporter;
            }

            if (xsdDCExporter.Options == null)
            {
                xsdDCExporter.Options = new ExportOptions();
            }

            if (xsdDCExporter.Options.DataContractSurrogate == null)
            {
                xsdDCExporter.Options.DataContractSurrogate = new DbusContainerSurrogate();
            }
        }

        public void ExportEndpoint(System.ServiceModel.Description.WsdlExporter exporter, System.ServiceModel.Description.WsdlEndpointConversionContext context)
        {

        }
        #endregion // IWsdlExportExtension Members

        #region IOperationBehavior Members
        public void AddBindingParameters(System.ServiceModel.Description.OperationDescription operationDescription, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            //
        }

        public void ApplyClientBehavior(System.ServiceModel.Description.OperationDescription operationDescription, System.ServiceModel.Dispatcher.ClientOperation clientOperation)
        {
            ApplyDataContractSurrogate(operationDescription);
        }

        public void ApplyDispatchBehavior(System.ServiceModel.Description.OperationDescription operationDescription, System.ServiceModel.Dispatcher.DispatchOperation dispatchOperation)
        {
            ApplyDataContractSurrogate(operationDescription);
        }

        public void Validate(System.ServiceModel.Description.OperationDescription operationDescription)
        {

        }
        #endregion // IOperationBehavior Members

        #region Implementation
        private static void ApplyDataContractSurrogate(System.ServiceModel.Description.OperationDescription description)
        {
            System.ServiceModel.Description.DataContractSerializerOperationBehavior dcsOperationBehavior =
                description.Behaviors.Find<System.ServiceModel.Description.DataContractSerializerOperationBehavior>();

            if (dcsOperationBehavior != null)
            {
                if (dcsOperationBehavior.DataContractSurrogate == null)
                {
                    dcsOperationBehavior.DataContractSurrogate = new DbusContainerSurrogate();
                }
            }
        }
        #endregion // Implementation
    } // Ends class DbusContainerAttribute
    #endregion // Surrogate Attribute
} // Ends namespace Udbus.WCF.Dbus.Contracts
