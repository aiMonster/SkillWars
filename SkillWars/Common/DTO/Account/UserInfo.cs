using Common.Entity;

namespace Common.DTO.Account
{
    public class UserInfo
    {
        public int Id { get; set; }
        public string NickName { get; set; }

        public UserInfo() { }

        public UserInfo(UserEntity entity)
        {
            Id = entity.Id;
            NickName = entity.NickName;
        }
    }
}
