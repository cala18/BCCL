

internal class Program
{
           //1. Devuelve un listado con el nombre de los todos los clientes españoles.
        public async Task<IEnumerable<Customer>> GetByCountry(string country)
        {
            return await _context.Customers
                .Where(c => c.Address.City.State.Country.Name == country)
                .ToListAsync();
        }

            //2. Devuelve un listado con los distintos estados por los que puede pasar un pedido.

    public async Task<IEnumerable<Object>> GetAllStatus()
    {
        var dato = await (
            from o in _context.Orders
            select new
            {
                Status = o.Status
            }
        ).Distinct()
        .ToListAsync();
        return dato;

    }

// #3 codigo 2008
          public async Task<IEnumerable<Customer>> GetIdByPaymentDate(int year)
        {
            return await _context.Customers
                                .Where(c => c.Orders.Any(o => o.Payment.PaymentDate.Year == year))
                                .Distinct()
                                .ToListAsync();
        }

            //9. Devuelve un listado con el código de pedido, código de cliente, fecha esperada y fecha de entrega de los pedidos que no han sido entregados a tiempo.
    public async Task<IEnumerable<Order>> GetAllNotDeliveredOnTime()
    {
        return await _context.Orders
                .Where(o => o.ExpectedDate  > o.DeliveryDate)
                .ToListAsync();
    }

    //10. Devuelve un listado con el código de pedido, código de cliente, fecha
    //esperada y fecha de entrega de los pedidos cuya fecha de entrega ha sido al
    //menos dos días antes de la fecha esperada.

        public async Task<IEnumerable<Order>> GetAllDeliveredEarlier()
    {
        return await _context.Orders
                .Where(o => (o.DeliveryDate.HasValue ? o.DeliveryDate.Value.Day + 2  : DateTime.MinValue.Day) <= o.ExpectedDate.Day &&
                o.DeliveryDate.HasValue)
                .ToListAsync();
    }

        //11. Devuelve un listado de todos los pedidos que fueron X en X.
    public async Task<IEnumerable<Order>> GetOrderByStatusYear(string status, int year)
    {
        return await _context.Orders
                .Where(o => o.Status.ToUpper() == status.ToUpper() && o.OrderDate.Year == year)
                .ToListAsync();
    }

    //12. Devuelve un listado de todos los pedidos que han sido (status X) en el mes  X de cualquier año.
    public async Task<IEnumerable<Order>> GetAllByMonth(string status, string Month)
    {
        List<Order> OrdersByMonth = new();
        if(DateOnly.TryParseExact(Month,"MMMM", CultureInfo.CurrentCulture, DateTimeStyles.None, out  DateOnly targetDate))
        {
            OrdersByMonth = await _context.Orders
                .Where(o => o.Status.ToUpper() == status.ToUpper() && targetDate.Month == o.OrderDate.Month)
                .ToListAsync();
        }
        return OrdersByMonth;

    }


        //13. Devuelve un listado con todos los pagos que se realizaron en el año X mediante X. Ordene el resultado de mayor a menor.
        public async Task<IEnumerable<Payment>> GetByPaymentMethodYear(string paymentMethod,int year)
        {
            return await _context.Payments
                                .Include(p => p.PaymentMethod)
                                .Where(p => p.PaymentMethod.Description.ToUpper() == paymentMethod.ToUpper() && p.PaymentDate.Year == year)
                                .OrderByDescending(p => p.Total) 
                                .ToListAsync(); 
        }

                //14. Devuelve un listado con todas las formas de pago que aparecen en la tabla pago. Tenga en cuenta que no deben aparecer formas de pago repetidas.
         //Por mi parte normalice los metodos de pago, así que solamente es necesario utilizar el metodo generico GetAllAsync.
        public override async Task<(int totalRegistros, IEnumerable<PaymentMethod> registros)> GetAllAsync(int pageIndex, int pageSize, string search)
            {
                var query = _context.PaymentMethods as IQueryable<PaymentMethod>;
    
                if(!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.Id.ToString() == search);
                }
    
                query = query.OrderBy(p => p.Id);
                var totalRegistros = await query.CountAsync();
                var registros = await query
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
    
                return (totalRegistros, registros);
            }
                   //15. Devuelve un listado con todos los productos que pertenecen a la gama Ornamentales y que tienen más de 100 unidades en stock. El listado deberá estar ordenado por su precio de venta, mostrando en primer lugar los de mayor precio.

        public async Task<IEnumerable<Product>> GetByGamaStock(string gama, int stock)
        {
            return await _context.Products
                                .Where(p => p.GamaNavigation.Gama == gama && p.StockQuantity > stock )
                                .OrderByDescending(p => p.SalePrice)
                                .ToListAsync();
        }

        // 16. Devuelve un listado con todos los clientes que sean de la ciudad de Madrid y cuyo representante de ventas tenga el código de empleado 11 o 30. 1.4.5 Consultas multitabla (Composición interna) Resuelva todas las consultas utilizando la sintaxis de SQL1 y SQL2. Las consultas con sintaxis de SQL2 se deben resolver con INNER JOIN y NATURAL JOIN.

        public async Task<IEnumerable<Customer>> GetByCityEmployee(string city, int employeeId1, int employeeId2)
        {
            return  await _context.Customers
                                .Where(c => c.Address.City.Name.ToUpper() == city.ToUpper() && c.Orders.All(o => o.EmployeeId == employeeId1 || o.EmployeeId == employeeId2))
                                .ToListAsync();
        }


    ///////////////////////////////////////////////////////////////////


        //1. Obtén un listado con el nombre de cada cliente y el nombre y apellido de su representante de ventas.
        public async Task<IEnumerable<object>> GetNameAndEmployee()
        {
            return await _context.Customers
                                .Include(c => c.Orders)
                                .ThenInclude(o => o.Employee)
                                .Select(c => new 
                                {
                                    c.Name,
                                    associatedEmployees = 
                                    c.Orders.Select(o => new
                                    {
                                        o.Employee.Name,
                                        LastName = o.Employee.LastName1 , o.Employee.LastName2
                                    }).Distinct()
                                })
                                .ToListAsync();
        }


                //2. Muestra el nombre de los clientes que hayan realizado pagos junto con el nombre de sus representantes de ventas.
        public async Task<IEnumerable<object>> GetByOrderEmployee()
        {
           return await _context.Customers
                                .Include(c => c.Orders)
                                .ThenInclude(o => o.Employee)
                                .Where(c => c.Orders.Any())
                                .Select(c => new 
                                {
                                    c.Name,
                                    associatedEmployees = 
                                    c.Orders.Select(o => new
                                    {
                                        o.Employee.Name,
                                    }).Distinct()
                                })
                                .ToListAsync();
        }

                //3. Muestra el nombre de los clientes que no hayan realizado pagos junto con el nombre de sus representantes de ventas.
        public async Task<IEnumerable<object>> GetByOrderNotPaymentEmployee()
        {
           return await _context.Customers
                                .Include(c => c.Orders)
                                .ThenInclude(o => o.Employee)
                                .Where(c => c.Orders.Any(o => o.PaymentId == null))
                                .Select(c => new 
                                {
                                    c.Name,
                                    associatedEmployees = 
                                    c.Orders.Select(o => new
                                    {
                                        o.Employee.Name,
                                    }).Distinct()
                                })
                                .ToListAsync();
        }

                // 4. Devuelve el nombre de los clientes que han hecho pagos y el nombre de sus representantes junto con la ciudad de la oficina a la que pertenece el representante.
        public async Task<IEnumerable<object>> GetByOrderPaymentEmployee()
        {
           return await _context.Customers
                                .Include(c => c.Orders)
                                .ThenInclude(o => o.Employee)
                                .Where(c => c.Orders.Any(o => o.PaymentId != null))
                                .Select(c => new 
                                {
                                    c.Name,
                                    associatedEmployees = 
                                    c.Orders.Select(o => new
                                    {
                                        o.Employee.Name,
                                        City = o.Employee.Office.Address.City.Name
                                    }).Distinct()
                                })
                                .ToListAsync();
        }

                //5. Devuelve el nombre de los clientes que no hayan hecho pagos y el nombre de sus representantes junto con la ciudad de la oficina a la que pertenece el representante.
        public async Task<IEnumerable<object>> GetByOrderNotPaymentEmployeeCity()
        {
            
           return await _context.Customers
                                .Include(c => c.Orders)
                                .ThenInclude(o => o.Employee)
                                .Where(c => c.Orders.Any(o => o.PaymentId == null))
                                .Select(c => new 
                                {
                                    c.Name,
                                    associatedEmployees = 
                                    c.Orders.Select(o => new
                                    {
                                        o.Employee.Name,
                                        City = o.Employee.Office.Address.City.Name
                                    }).Distinct()
                                })
                                .ToListAsync();
        }

                //6. Devuelve un listado que muestre el nombre de cada empleado, el nombre de su jefe y el nombre del jefe de su jefe.
        public async Task<IEnumerable<object>> GetNameAndBossChief()
        {
           return await _context.Employees
                                .Select(c => new 
                                {
                                    c.Name,
                                    Boss = c.Boss.Name,
                                    Chief = c.Boss.Boss.Name    

                                })
                                .ToListAsync();
        }
                //7. Devuelve el nombre de los clientes a los que no se les ha entregado a tiempo un pedido.
        public async Task<IEnumerable<Customer>> GetNameNoDeliveryOnTime()
        {
           return await _context.Customers
                                .Where(c => c.Orders.Any(o => o.DeliveryDate > o.ExpectedDate))                                
                                .ToListAsync();
        }

                //8. Devuelve un listado de las diferentes gamas de producto que ha comprado cada cliente. 1.4.6 Consultas multitabla (Composición externa) Resuelva todas las consultas utilizando las cláusulas LEFT JOIN, RIGHT JOIN, NATURAL LEFT JOIN y NATURAL RIGHT JOIN.
        public async Task<IEnumerable<object>> GetByProductGama()
        {
            return await _context.Products
                                .Include(p => p.GamaNavigation)
                                .Select(p => new{ProductGama = p.GamaNavigation.Gama})
                                .Distinct()
                                .ToListAsync();
        }

        //////////////////////////////////////////////////////////////////
        ///
                //2. Devuelve un listado que muestre los clientes que no han realizado ningún pago y los que no han realizado ningún pedido.
        public async Task<IEnumerable<Customer>> GetByNotPaidAndNotOrder()
        {
           return await _context.Customers
                                .Where(c => !c.Orders.Any())
                                .ToListAsync();
         
        }

              //3. Devuelve un listado que muestre solamente los empleados que no tienen un cliente asociado junto con los datos de la oficina donde trabajan

        public async Task<IEnumerable<Employee>> GetNotAssociatedEmployeeOffice()
        {
           return await _context.Employees
                                .Include(e => e.Office)
                                .ThenInclude(o => o.Address)
                                .Where(e => !e.Orders.Any()).ToListAsync();
        }

               //4. Devuelve un listado que muestre los empleados que no tienen una oficina asociada y los que no tienen un cliente asociado.
        public async Task<IEnumerable<Employee>> GetNotAssociatedEmployeeAndOffice()
        {
            return await _context.Employees
                                .Include(e => e.Office)
                                .ThenInclude(o => o.Address)
                                .Where(e => e.OfficeId == null || !e.Orders.Any()).ToListAsync();
        }
                //5. Devuelve un listado de los productos que nunca han aparecido en un pedido.
        public async Task<IEnumerable<Product>> GetNeverInOrder()
        {
            return await _context.Products
                                .Where(p => !p.OrderDetails.Any())
                                .ToListAsync();
        }

                //6. Devuelve un listado de los productos que nunca han aparecido en un pedido. El resultado debe mostrar el nombre, la descripción y la imagen del producto.
        public async Task<IEnumerable<object>> GetNeverInOrderspecified()
        {
            return await _context.Products
                                .Where(p => !p.OrderDetails.Any())
                                .Select(p => new{
                                    p.Name,
                                    p.Description,
                                    p.GamaNavigation.Image
                                }).ToListAsync();
        }

               // 7. Devuelve las oficinas donde no trabajan ninguno de los empleados que hayan sido los representantes de ventas de algún cliente que haya realizado la compra de algún producto de la gama Frutales.
        public async Task<IEnumerable<Office>> GetByEmployeeWithProductGama(string gama)
        {
            return await _context.Offices
                            .Include(o => o.Address)
                            .ThenInclude(a => a.City)
                            .Where(o => o.Employees
                            .Any(e => e.JobTitle.ToUpper() == "REPRESENTANTE VENTAS") &&
                            !o.Employees.Any(e => e.Orders.Any(o => o.OrderDetails
                            .Any(od => od.Product.GamaNavigation.Gama.Equals(gama))))).ToListAsync();
        }
    
            
        //9. Devuelve un listado con los datos de los empleados que no tienen clientes asociados y el nombre de su jefe asociado.
        public async Task<IEnumerable<Employee>> GetNotAssociatedcustomerBossName()
        {
            return await _context.Employees
                                .Include(e => e.Boss)
                                .Where(e => !e.Orders.Any())
                                .ToListAsync();
        }


        /////////////////////////////////////////////////////////////////////
        ///

                //1. ¿Cuántos empleados hay en la compañía?
        public async Task<object> GetEmployeesQuantity()
        {
            return new { EmployeesQuantity = await _context.Employees.CountAsync()};
        }

                        //2. ¿Cuántos clientes tiene cada país?
        public async Task<IEnumerable<object>> GetCustomersQuantityByCountry()
        {
            return await _context.Countries
                                .Select(c => new{
                                    c.Name,
                                    CustomersQuantity = 
                                    c.States.SelectMany(s => s.Cities
                                            .SelectMany(c => c.Addresses
                                            .Select(a => a.Customers.Count)
                                            )).Sum()
                                })
                                .ToListAsync();
         
        }

          //3. ¿Cuál fue el pago medio en 2009?
    public async Task<object> GetOrderPaymentAverangeInYear(int year)
    {
        return new {PaymentAverange =  await _context.Orders.Where(o => o.OrderDate.Year == year).AverageAsync( p => p.Payment.Total)};
    }

          //4. ¿Cuántos pedidos hay en cada estado? Ordena el resultado de forma descendente por el número de pedidos.
    public async Task<object> GetOrdersQuantityByStatus ()
    {
    var result = await _context.Orders
        .GroupBy(o => o.Status)
        .Select(g => new{ Status = g.Key, orderQuantity = g.Count()}).OrderByDescending(o => o.orderQuantity)
        .ToListAsync();

    return result;
    } 

    
        //5. ¿Cuántos clientes existen con domicilio en la ciudad de Madrid?
        public async Task<object> GetByCustomerQuantityInCity(string city)
        {
            return  new {customerQuantity = await _context.Customers.Where(c => c.Address.City.Name.ToUpper() == city.ToUpper()).CountAsync()};
        }

            //6. ¿Calcula cuántos clientes tiene cada una de las ciudades que empiezan por M?
        public async Task<object> GetByCustomerQuantityInLetterCity(string letter)
        {
            return  new {customerQuantity = await _context.Customers
            .Where(c => c.Address.City.Name.ToUpper().StartsWith(letter.ToUpper())).CountAsync()};
        }

       //7. Devuelve el nombre de los representantes de ventas y el número de clientes
        public async Task<IEnumerable<object>> GetEmployeesCustomerQuantity()
        {
            return await _context.Employees
                                .Where(e => e.JobTitle == "REPRESENTANTE VENTAS")
                                .GroupBy(e => e.Orders.FirstOrDefault().CustomerId)
                                .Select(g => new{
                                    g.FirstOrDefault().Name,
                                    customersAsociated = g.Count()
                                }).ToListAsync();
        }
                //8. Calcula el número de clientes que no tiene asignado representante de ventas.
        public async Task<object> GetByNotAssignedEmployee()
        {
        return new { CustomerQuantity = await _context.Customers.Where(c => !c.Orders.Any()).CountAsync()};
        }

        //9. Calcula la fecha del primer y último pago realizado por cada uno de los clientes. El listado deberá mostrar el nombre y los apellidos de cada cliente.
        public async Task<IEnumerable<object>> GetFirstLastPaymentByCustomer()
        {
            return await _context.Customers
                                .Where(c => c.Orders.Any(o => o.PaymentId != null))
                                .Select( c => new{
                                    firstPayment = c.Orders.Min(o => o.Payment.PaymentDate),
                                    LastPayment = c.Orders.Max(o => o.Payment.PaymentDate),
                                    c.Name,
                                    lastName = c.ContactName + " " + c.ContactLastName
                                }).ToListAsync();
        }

            //10. Calcula el número de productos diferentes que hay en cada uno de los pedidos.
    
    public async Task<object> GetByDifferentProdQuantity()
    {
        return new{ DiffProductQuantity = await _context.Orders.Select(o => o.OrderDetails.Select(od => od.ProductId).Distinct()).CountAsync()};
    }

       //11. Calcula la suma de la cantidad total de todos los productos que aparecen en cada uno de los pedidos.
    public async Task<object> GetTotalSumProdInOrder()
    {
        return await _context.OrderDetails
                            .GroupBy(od => od.OrderId)
                            
                            .Select(g => new
                            { 
                                OrderId =g.Key,
                                TotalProductSum = g.Sum(p => Convert.ToInt32(p.Cantidad))
                            }).ToListAsync();               
    }

        //12. Devuelve un listado de los 20 productos más vendidos y el número total de unidades que se han vendido de cada uno. El listado deberá estar ordenado por el número total de unidades vendidas.
        public async Task<IEnumerable<object>> GetMostSold()
        {
            return await _context.OrderDetails
                            .GroupBy(od => od.ProductId)
                            
                            .Select(g => new
                            { 
                                g.FirstOrDefault().Product.Name, 
                                g.FirstOrDefault().Product.GamaNavigation.Gama,
                                g.FirstOrDefault().Product.Supplier,
                                g.FirstOrDefault().Product.Description,
                                TotalProductSum = g.Sum(p => Convert.ToInt32(p.Cantidad))
                            })
                            .OrderByDescending(p => p.TotalProductSum)
                            .Take(20)
                            .ToListAsync();               

        }
 // 13

         public async Task<IEnumerable<object>> GetMostSoldGroupedByCod()
        {
            return await _context.OrderDetails
                            .GroupBy(od => od.ProductId)
                            
                            .Select(g => new
                            { 
                                g.FirstOrDefault().Product.Id,
                                g.FirstOrDefault().Product.Name, 
                                g.FirstOrDefault().Product.GamaNavigation.Gama,
                                g.FirstOrDefault().Product.Supplier,
                                g.FirstOrDefault().Product.Description,
                                TotalProductSum = g.Sum(p => Convert.ToInt32(p.Cantidad))
                            })
                            .OrderByDescending(p => p.Id)
                            .Take(20)
                            .ToListAsync();               
        }

   //14. La misma información que en la pregunta anterior, pero agrupada por código de producto filtrada por los códigos que empiecen por OR. 
        public async Task<IEnumerable<object>> GetMostSoldGroupedByCodFiltered(string letters)
        {
            return await _context.OrderDetails
                            .Where(od => od.ProductId.ToUpper().StartsWith(letters.ToUpper()))
                            .GroupBy(od => od.ProductId)
                            
                            .Select(g => new
                            { 
                                g.FirstOrDefault().Product.Id,
                                g.FirstOrDefault().Product.Name, 
                                g.FirstOrDefault().Product.GamaNavigation.Gama,
                                g.FirstOrDefault().Product.Supplier,
                                g.FirstOrDefault().Product.Description,
                                TotalProductSum = g.Sum(p => Convert.ToInt32(p.Cantidad))
                            })
                            .OrderByDescending(p => p.Id)
                            .Take(20)
                            .ToListAsync();               
        }
        //15. Lista las ventas totales de los productos que hayan facturado más de X euros. Se mostrará el nombre, unidades vendidas, total facturado y total facturado con impuestos (21% IVA).
        public async Task<IEnumerable<object>> GetTotalSaleByQuantityRange(int range)
        {
            return await _context.OrderDetails
                            .GroupBy(od => od.ProductId)
                            .Select(g => new
                            { 
                                g.FirstOrDefault().Product.Name,
                                g.FirstOrDefault().UnitPrice,
                                Total = g.Sum(od => Math.Round(Convert.ToInt32(g.FirstOrDefault().Cantidad) * od.UnitPrice, 2)),
                                TotalWithTaxes = g.Sum(od => Math.Round(Convert.ToInt32(g.FirstOrDefault().Cantidad) * od.UnitPrice , 2)) * (decimal)1.21
                            })
                            .Where(g => g.Total > range)
                            .OrderByDescending(g => g.Total)
                            .ToListAsync();
        }
        //16. Muestre la suma total de todos los pagos que se realizaron para cada uno de los años que aparecen en la tabla pagos.

        public async Task<IEnumerable<object>> GetOrderTotalSumByYear()
        {
            return await _context.Orders
                                .Where(o => o.PaymentId != null)
                                .GroupBy(o => o.Payment.PaymentDate)
                                .Select(g => new
                                {
                                    PaymentDate = g.Key,
                                    Total = g.Sum(o => o.Payment != null ? o.Payment.Total : 0)
                                })
                                .OrderByDescending(n => n.Total)
                                .ToListAsync();
        }
        //////////////////////////////////////////////
        ///
               //1. Devuelve el nombre del cliente con mayor límite de crédito
        //8. Devuelve el nombre del cliente con mayor límite de crédito. En linq tanto Any como All esperaran una condicion para hacer un filtro, no vi manera de solucionarlo con alguno de estos metodos.
        
        public async Task<Customer> GetByGreatestCreditLimit()
        {
            return await _context.Customers 
                                .OrderByDescending(c => c.CreditLimit)
                                .FirstOrDefaultAsync();
        }

                //2. Devuelve el nombre del producto que tenga el precio de venta más caro.
        public async Task<Product> GetByHigherSalesPrice()
        {
            return await _context.Products
                                .OrderByDescending(p => p.SalePrice)
                                .FirstOrDefaultAsync();
        }
        //3. Devuelve el nombre del producto del que se han vendido más unidades.

        public async Task<object> GetByHigherUnitsPrice()
        {
            return await _context.OrderDetails
                                .GroupBy( od => od.ProductId)
                                .Select(g => new
                                {
                                    g.FirstOrDefault().Product.Name,
                                    TotalUnits = g.Sum(od => Convert.ToInt32(od.Cantidad))
                                })
                                .OrderByDescending(p => p.TotalUnits)
                                .FirstOrDefaultAsync();
        }
                //4. Los clientes cuyo límite de crédito sea mayor que los pagos que haya realizado. (Sin utilizar INNER JOIN).
        public async Task<IEnumerable<Customer>> GetByHigherCreditLimitThanPayment()
        {
            return await _context.Customers
                                .Where(c => c.Orders.Any(o => o.PaymentId != null ))
                                .Where(c => c.CreditLimit > c.Orders.Sum(o => o.Payment.Total))
                                .ToListAsync();
        }
/////////////////////////////////////////////////
///

       //8. Devuelve el nombre del cliente con mayor límite de crédito. En linq tanto Any como All esperaran una condicion para hacer un filtro, no vi manera de solucionarlo con alguno de estos metodos.
        
        public async Task<Customer> GetByGreatestCreditLimit()
        {
            return await _context.Customers 
                                .OrderByDescending(c => c.CreditLimit)
                                .FirstOrDefaultAsync();
        }
                //9. Devuelve el nombre del producto que tenga el precio de venta más caro.
        public async Task<Product> GetByHigherSalesPrice()
        {
            return await _context.Products
                                .OrderByDescending(p => p.SalePrice)
                                .FirstOrDefaultAsync();
        }
                //11. Devuelve un listado que muestre solamente los clientes que no han realizado ningún pago.
        public async Task<IEnumerable<Customer>> GetByNotOrder()
        
        {
            return await _context.Customers 
                                .Where(c => c.Orders.Any(o => o.Payment == null))
                                .ToListAsync();
        }
                //12. Devuelve un listado que muestre solamente los clientes que sí han realizado
        public async Task<IEnumerable<Customer>> GetByOrderPaid()
        {
            return await _context.Customers 
                                .Where(c => c.Orders.Any(o => o.Payment != null))
                                .ToListAsync();
        }

       //13. Devuelve un listado de los productos que nunca han aparecido en un pedido.
        public async Task<IEnumerable<Product>> GetByNotInOrder()
        {
            return await _context.Products
                                .Where(p => !p.OrderDetails.Any())
                                .ToListAsync();
        }

                //14. Devuelve el nombre, apellidos, puesto y teléfono de la oficina de aquellos empleados que no sean representante de ventas de ningún cliente.
        public async Task<IEnumerable<object>> GetEmployeesWithoutOrder()
        {
            return await _context.Employees
                                .Where(e => e.JobTitle == "REPRESENTANTE VENTAS"
                                && !e.Orders.Any())
                                .Select(e => new{
                                    e.Name,
                                    LastName = e.LastName1 + " " + e.LastName2,
                                    e.JobTitle,
                                    e.Office.Phone
                                }).ToListAsync();
        }
        ////////////////////7
        ///
        
               //1. Devuelve el listado de clientes indicando el nombre del cliente y cuántos pedidos ha realizado. Tenga en cuenta que pueden existir clientes que no han realizado ningún pedido.

        public async Task<IEnumerable<object>> GetNameAndOrdersQuantity()
        {
            return await _context.Customers
                                .Select(c => new
                                {
                                    c.Name,
                                    c.Orders.Count
                                }).ToListAsync();
        }

                //2. Devuelve el nombre de los clientes que hayan hecho pedidos en 2008 ordenados alfabéticamente de menor a mayor.
        public async Task<IEnumerable<Customer>> GetByOrderInYear(int year)
        {
            return await _context.Customers
                                .Where(c => c.Orders.All(o => o.OrderDate.Year > year))
                                .OrderBy(c => c.Name)
                                .ToListAsync();
        }
        //3. Devuelve el nombre del cliente, el nombre y primer apellido de su representante de ventas y el número de teléfono de la oficina del representante de ventas, de aquellos clientes que no hayan realizado ningún pago.
        public async Task<IEnumerable<object>> GetDataAndEmployee()
        {
            return await _context.Customers
                                .Where(c => c.Orders.Any(o => o.PaymentId == null))
                                .Select(c => new
                                {
                                    c.Name,
                                    AssociatedEmployees = c.Orders.Select(o => new
                                    {
                                        o.Employee.Name,
                                        o.Employee.LastName1,
                                        OfficeNumber = o.Employee.Office.Phone
                                    })
                                }).ToListAsync();
        }
        //4. Devuelve el listado de clientes donde aparezca el nombre del cliente, el nombre y primer apellido de su representante de ventas y la ciudad donde está su oficina.
        public async Task<IEnumerable<object>> GetDataAndEmployeeCity()
        {
            return await _context.Customers
                                .Select(c => new
                                {
                                    c.Name,
                                    AssociatedEmployees = c.Orders.Select(o => new
                                    {
                                        o.Employee.Name,
                                        o.Employee.LastName1,
                                        OfficeCity = o.Employee.Office.Address.City.Name
                                    })
                                }).ToListAsync();
        }
          //5. Devuelve el nombre, apellidos, puesto y teléfono de la oficina de aquellos empleados que no sean representante de ventas de ningún cliente
        public async Task<IEnumerable<object>> GetDataByJobTitle()
        {
            return await _context.Employees
                                .Where(e => !e.Orders.Any())
                                .Where(e => e.JobTitle == "REPRESENTANTE VENTAS"
                                && !e.Orders.Any())
                                .Select(e => new{
                                    e.Name,
                                    LastName = e.LastName1 + " " + e.LastName2,
                                    e.JobTitle,
                                    e.Office.Phone
                                }).ToListAsync();
        }
        
}