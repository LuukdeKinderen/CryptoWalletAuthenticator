using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWalletAuthenticator
{
    public class User
    {
        [Key]
        public string WalletAdress { get; set; }
        public string Nonce { get; set; }
        public string FavoriteColour {  get; set; }
    }
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options)
        : base(options)
        { }
        public DbSet<User> Users { get; set; }
    }
}
