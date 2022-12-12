pragma solidity >0.7.0;
/*
Requirements:
В системе должна храниться информация о различных объектах недвижимости, а именно:
- владелец (аккаунт в сети Ethereum)
- физический адрес
- площадь объекта
- наличие обременений
В системе должен присутствовать администратор, один из аккаунтов сети - автор смарт-контракта.
При старте системы в ней должен присутствовать объект недвижимости:
- Таганрог, ул. Чехова, дом 2, общей площадью 1000 кв.м, без обременений, владелец - произвольный из списка аккаунтов.
Функционал системы:
- добавление объекта недвижимости (только админом)
- изменение владельца (только админом)
- изменение площади (только админом)
- наложение обременений (только админом)
- создание сущности "подарка" - владелец создает с указанием кому подарить, при подтверждении получения сменяется собственник
- создание сущности "продажа" с заявленной стоимостью - владелец выставляет на продажу, покупатель откликается, 
- при успешном переводе средств сменяется собственник, объявление становится неактуальнымдарить и продавать собственность с обременением нельзя
*/
contract REAgency{

    struct RealEstate{
        address owner;
        string reAddress;
        uint area;
        bool restricted;
    }

    struct PresentOffer {
        uint reId;
        address to;
        bool accepted;
    }

    struct SellOffer {
        uint reId;
        uint price;
        bool sold;
    }

    address admin;

    mapping(uint => RealEstate) public reList;
    mapping(uint => PresentOffer) public presentsList;
    mapping(uint => SellOffer) public salesList;

    constructor() {
        admin = msg.sender;
        reList[111] = RealEstate(admin, "Taganrog, Chekhova st., 2", 1000, false);
    }

    modifier root() {
        require(msg.sender == admin, "You are not admin");
        _;
    }

    function getRealEstate(uint _id) public view returns (address owner, string memory reAddress, uint area, bool restricted) {
        return (reList[_id].owner, reList[_id].reAddress, reList[_id].area, reList[_id].restricted);
    }

    function addRealEstate(uint _id, address _owner, string memory _reAddress, uint _area, bool _restricted) public root {
        require(_id != 0);
        require(reList[_id].owner == address(0), "This id was already taken");
        require(_owner != address(0));
        require(_area > 0);
        reList[_id] = RealEstate(_owner, _reAddress, _area, _restricted);
    }

    function changeOwner(uint _id, address _newOwner) public root {
        require(_id != 0);
        reList[_id].owner = _newOwner;
    }

    function changeArea (uint _id, uint _newArea) public root {
        require(_id != 0);
        require(_newArea > 0, "Area must be bigger than 0");
        reList[_id].area = _newArea;
    }

    function setRestrictions (uint _id, bool _restricted) public root {
        require(_id != 0);
        reList[_id].restricted = _restricted;
    }

    // ===============================================================================================================================

    modifier validRE(uint _id) {
        require(reList[_id].owner != address(0), "Real estate is not valid");
        require(reList[_id].restricted == false, "Real estate is restricted");
        _;
    }

    function sendPresent (uint _id, address _to) public validRE(_id) {
        require(_id != 0);
        require(_to != address(0));
        require(msg.sender == reList[_id].owner, "You are not the owner");
        require(reList[_id].owner != _to, "Can't send to the owner");

        PresentOffer storage newPresent = presentsList[_id];
        newPresent.reId = _id;
        newPresent.to = _to;
        newPresent.accepted = false;
    }

    function acceptPresent(uint _id) public validRE(_id) {
        require(_id != 0);
        require(presentsList[_id].accepted == false, "Present is already accepted");
        require(msg.sender == presentsList[_id].to, "Sender is not the recepient of the present");

        reList[presentsList[_id].reId].owner = msg.sender;
        presentsList[_id].accepted = true;
    }

    function sendSellOffer(uint _id, uint _price) public validRE(_id) {
        require(_id != 0);
        require(_price > 0);
        require(msg.sender == reList[_id].owner, "You are not the owner");

        SellOffer storage newSale = salesList[_id];
        newSale.reId = _id;
        newSale.price = _price;
        newSale.sold = false;
    }

    function acceptSellOffer(uint _id) public payable validRE(_id) {
        require(_id != 0);
        require(salesList[_id].sold == false, "Sale offer was already accepted");
        require(reList[salesList[_id].reId].owner != msg.sender, "You already own this real estate");
        require(msg.value == salesList[_id].price, "Value does not meet price of the real estate");

        payable(reList[salesList[_id].reId].owner).transfer(msg.value);
        reList[salesList[_id].reId].owner = msg.sender;
        salesList[_id].sold = true;
    }
}