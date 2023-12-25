using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility.Interface;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Utility.Implementation
{
    public class VillaNumberService : IVillaNumberService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VillaNumberService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public bool CheckVillaNumberExists(int villa_Number)
        {
            return _unitOfWork.VillaNumber.Any(u => u.Villa_Number == villa_Number);
        }

        public void CreateVillaNumber(VillaNumber villaNumber)
        {
            _unitOfWork.VillaNumber.Add(villaNumber); //write a script to create the VillaNumber object in the database
            _unitOfWork.Save(); //Go into the database and create the VillaNumber object
        }

        public bool DeleteVillaNumber(int id)
        {
            try
            {
                VillaNumber? objFromDb = _unitOfWork.VillaNumber.Get(u => u.Villa_Number == id);
                if (objFromDb is not null)
                {
                    _unitOfWork.VillaNumber.Remove(objFromDb); //write a script to delete the VillaNumber object in the database
                    _unitOfWork.Save(); //Go into the database and delete the VillaNumber object
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public IEnumerable<VillaNumber> GetAllVillaNumbers(bool isPropertiesIncluded = true)
        {
            string includeProps = string.Empty;
            if (isPropertiesIncluded)
            {
                includeProps = "Villa";
            }
            return _unitOfWork.VillaNumber.GetAll(includeProperties: includeProps);
        }

        public VillaNumber GetVillaNumberById(int id, bool isPropertiesIncluded = true)
        {
            string includeProps = string.Empty;
            if (isPropertiesIncluded)
            {
                includeProps = "Villa";
            }
            return _unitOfWork.VillaNumber.Get(u => u.Villa_Number == id, includeProperties: includeProps);
        }

        public void UpdateVillaNumber(VillaNumber villaNumber)
        {
            _unitOfWork.VillaNumber.Update(villaNumber); //write a script to update the VillaNumber object in the database
            _unitOfWork.Save(); //Go into the database and update the VillaNumber object        }
        }
    }
}
