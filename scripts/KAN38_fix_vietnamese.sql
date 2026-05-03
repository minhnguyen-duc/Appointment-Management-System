-- KAN-38: Fix garbled Vietnamese names in database
-- Run in SSMS after confirming encoding issue

USE AppointmentAMS;
GO

-- Fix Doctors table
UPDATE Doctors SET FullName = N'Nguyễn Văn An',   Bio = N'Chuyên gia tim mạch can thiệp với hơn 15 năm kinh nghiệm. Từng tu nghiệp tại Pháp và Hoa Kỳ.'    WHERE LicenseNumber = 'LIC-001';
UPDATE Doctors SET FullName = N'Trần Thị Bình',   Bio = N'Phó Giáo sư chuyên về nhi khoa sơ sinh. Kinh nghiệm 20 năm điều trị bệnh lý trẻ em.'              WHERE LicenseNumber = 'LIC-002';
UPDATE Doctors SET FullName = N'Lê Minh Châu',    Bio = N'Bác sĩ chuyên khoa I da liễu. Điều trị các bệnh da mãn tính, thẩm mỹ da.'                         WHERE LicenseNumber = 'LIC-003';
UPDATE Doctors SET FullName = N'Phạm Thị Dung',   Bio = N'Tiến sĩ Thần kinh học, chuyên về đột quỵ não và Parkinson. 18 năm kinh nghiệm lâm sàng.'          WHERE LicenseNumber = 'LIC-004';
UPDATE Doctors SET FullName = N'Hoàng Văn Em',    Bio = N'Bác sĩ chuyên khoa II Tai Mũi Họng. Phẫu thuật nội soi mũi xoang.'                                WHERE LicenseNumber = 'LIC-005';
UPDATE Doctors SET FullName = N'Vũ Thị Phương',   Bio = N'Chuyên gia tiêu hóa – nội soi can thiệp. Điều trị viêm loét dạ dày, viêm đại tràng.'              WHERE LicenseNumber = 'LIC-006';
UPDATE Doctors SET FullName = N'Đặng Minh Quân',  Bio = N'Bác sĩ nhãn khoa. Phẫu thuật khúc xạ laser, điều trị mắt hột, đục thủy tinh thể.'                WHERE LicenseNumber = 'LIC-007';
UPDATE Doctors SET FullName = N'Ngô Thị Hoa',     Bio = N'Phó Giáo sư nội khoa. Chẩn đoán và điều trị bệnh nội khoa phức tạp.'                              WHERE LicenseNumber = 'LIC-008';
UPDATE Doctors SET FullName = N'Bùi Văn Sơn',     Bio = N'Chuyên khoa I Cơ xương khớp. Điều trị viêm khớp, thoái hóa khớp, loãng xương.'                  WHERE LicenseNumber = 'LIC-009';
UPDATE Doctors SET FullName = N'Cao Thị Lan',     Bio = N'Tiến sĩ Nhi khoa, chuyên về miễn dịch – dị ứng trẻ em. 12 năm kinh nghiệm.'                      WHERE LicenseNumber = 'LIC-010';

-- Fix Specializations
UPDATE Doctors SET Specialization = N'Tim mạch'       WHERE LicenseNumber = 'LIC-001';
UPDATE Doctors SET Specialization = N'Nhi khoa'       WHERE LicenseNumber IN ('LIC-002','LIC-010');
UPDATE Doctors SET Specialization = N'Da liễu'        WHERE LicenseNumber = 'LIC-003';
UPDATE Doctors SET Specialization = N'Thần kinh'      WHERE LicenseNumber = 'LIC-004';
UPDATE Doctors SET Specialization = N'Tai Mũi Họng'   WHERE LicenseNumber = 'LIC-005';
UPDATE Doctors SET Specialization = N'Tiêu hóa'       WHERE LicenseNumber = 'LIC-006';
UPDATE Doctors SET Specialization = N'Mắt'            WHERE LicenseNumber = 'LIC-007';
UPDATE Doctors SET Specialization = N'Nội tổng quát'  WHERE LicenseNumber = 'LIC-008';
UPDATE Doctors SET Specialization = N'Cơ xương khớp'  WHERE LicenseNumber = 'LIC-009';

-- Fix Patients
UPDATE Patients SET FullName = N'Nguyễn Mai'      WHERE PhoneNumber = '0912345678';
UPDATE Patients SET FullName = N'Trần Văn Bình'   WHERE PhoneNumber = '0987654321';
UPDATE Patients SET FullName = N'Lê Thị Cúc'      WHERE PhoneNumber = '0909123456';

-- Fix PatientProfiles
UPDATE PatientProfiles SET FullName = N'Nguyễn Mai',  Gender = N'Nữ',  Relation = N'Bản thân' WHERE Id = 'C3000001-0000-0000-0000-000000000001';
UPDATE PatientProfiles SET FullName = N'Trần Thị Hoa', Gender = N'Nữ', Relation = N'Mẹ'       WHERE Id = 'C3000001-0000-0000-0000-000000000002';

SELECT Id, FullName, Specialization, AcademicTitle FROM Doctors ORDER BY LicenseNumber;
GO
