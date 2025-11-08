// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 39d87d14ab1ef59388dbab3d54f6bb3fd8df13210fe759c67f4edb905bc3eaf0
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Exceptions;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class LocationDiagramManager : ManagerBase, ILocationDiagramManager
    {
        private readonly ILocationDiagramRepo _locationDiagramRepo;
        private readonly IOrgLocationRepo _orgLocationRepo;


        public LocationDiagramManager(ILocationDiagramRepo locationDiagramRepo, IOrgLocationRepo orgLocationRepo, ILogger logger, IAppConfig appConfig, IDependencyManager dependencyManager, ISecurity security) : base(logger, appConfig, dependencyManager, security)
        {
            _locationDiagramRepo = locationDiagramRepo ?? throw new ArgumentNullException(nameof(locationDiagramRepo));
            _orgLocationRepo = orgLocationRepo ?? throw new ArgumentNullException(nameof(orgLocationRepo));
        }

        public async Task<InvokeResult> AddLocationDiagramAsync(LocationDiagram diagramLocation, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(diagramLocation, Actions.Create);
            await AuthorizeAsync(diagramLocation, AuthorizeResult.AuthorizeActions.Create, user, org);

            await _locationDiagramRepo.AddLocationDiagramAsync(diagramLocation);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> DeleteLocationDiagramAsync(string id, EntityHeader org, EntityHeader user)
        {
            var locationDiagram = await _locationDiagramRepo.GetLocationDiagramAsync(id);
            await AuthorizeAsync(locationDiagram, AuthorizeResult.AuthorizeActions.Delete, user, org);
            await _locationDiagramRepo.DeleteLocationDiagramAsync(id);
            return InvokeResult.Success;
        }

        public async Task<LocationDiagram> GetLocationDiagramAsync(string id, EntityHeader org, EntityHeader user)
        {
            var diagram = await _locationDiagramRepo.GetLocationDiagramAsync(id);
            await AuthorizeAsync(diagram, AuthorizeResult.AuthorizeActions.Read, user, org);

            return diagram;
        }

        public async Task<ListResponse<LocationDiagramSummary>> GetLocationDiagramsAsync(ListRequest listRequest, EntityHeader org, EntityHeader user )
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(LocationDiagram));
            return await _locationDiagramRepo.GetLocationDiagramsAsync(org.Id, listRequest);
        }

        public async Task<ListResponse<LocationDiagramSummary>> GetLocationDiagramsForCustomerAsync(string customerId, ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(LocationDiagram));
            return await _locationDiagramRepo.GetLocationDiagramsForCustomerAsync(org.Id, customerId, listRequest);
        }

        public async Task<InvokeResult> UpdateLocationDiagramAsync(LocationDiagram diagramLocation, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(diagramLocation, Actions.Create);
            await AuthorizeAsync(diagramLocation, AuthorizeResult.AuthorizeActions.Update, user, org);

            foreach(var layer in diagramLocation.Layers)
            {
                /*foreach(var shape in layer.Shapes)
                {
                    if(!EntityHeader.IsNullOrEmpty(shape.Location))
                    {
                        var location = await _orgLocationRepo.GetLocationAsync(shape.Location.Id);
                        if (location.OwnerOrganization.Id != org.Id)
                            throw new NotAuthorizedException("Attempt to associate location from a different organization.");

                        var existing = location.DiagramReferences.Where(dg=>dg.LocationDiagramShape.Id == shape.Id).FirstOrDefault();
                        if(existing == null)
                        {
                            location.DiagramReferences.Add(new OrgLocationDiagramReference()
                            {
                                LocationDiagram = diagramLocation.ToEntityHeader(),                               
                                LocationDiagramLayer = layer.ToEntityHeader(),
                                LocationDiagramShape = shape.ToEntityHeader()
                            });

                            await _orgLocationRepo.UpdateLocationAsync(location);
                        }    
                    }
                }*/
            }

            await _locationDiagramRepo.UpdateLocationDiagramAsync(diagramLocation);

            return InvokeResult.Success;
        }
    }
}
