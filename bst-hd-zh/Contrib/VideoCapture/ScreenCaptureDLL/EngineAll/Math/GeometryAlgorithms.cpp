/*
GeometryAlgorithms.cpp
Written by Matthew Fisher
*/
#include "..\\..\\Main.h"
#include "GeometryAlgorithms.h"

void GeometryAlgorithms::ComputeRigidCorrespondenceTransform(Matrix4 &SourceToTargetTransform, const Vector<RigidCorrespondenceEntry> &Correspondences)
{
    PersistentAssert(Correspondences.Length() > 3, "Invalid input"); 

	/*
	The solution is based on
	Berthold K. P. Horn (1987),
	"Closed-form solution of absolute orientation using unit quaternions,"
	Journal of the Optical Society of America A, 4:629-642

	The code is modelled after vtkLandmarkTransform in the Visualization Toolkit
	*/

	UINT PointCount = Correspondences.Length();

	// -- find the centroid of each set --

	Vec3f SourceCentroid = Vec3f::Origin;
	Vec3f TargetCentroid = Vec3f::Origin;
	float WeightSum = 0.0f;
	for(UINT PointIndex = 0; PointIndex < PointCount; PointIndex++)
	{
		const RigidCorrespondenceEntry &CurEntry = Correspondences[PointIndex];
		SourceCentroid += CurEntry.Source * CurEntry.Weight;
		TargetCentroid += CurEntry.Target * CurEntry.Weight;
		WeightSum += CurEntry.Weight;
	}
	SourceCentroid /= WeightSum;
	TargetCentroid /= WeightSum;
	
	float M[3][3];
	for(UINT i = 0; i < 3; i++)
	{
		M[i][0] = 0.0f;
		M[i][1] = 0.0f; 
		M[i][2] = 0.0f; 
	}
	
	//
	// We do not use scale factors, but might eventually
	//
	//float sa = 0.0f, sb = 0.0f;

	for(UINT PointIndex = 0; PointIndex < PointCount; PointIndex++)
	{
		const RigidCorrespondenceEntry &CurEntry = Correspondences[PointIndex];
		Vec3f SourcePt = CurEntry.Source - SourceCentroid;
		Vec3f TargetPt = CurEntry.Target - TargetCentroid;
 
		//
		// accumulate the products a*T(b) into the matrix M
		//
		for(UINT i = 0; i < 3; i++)
		{
			M[i][0] += CurEntry.Weight * SourcePt[i] * TargetPt[0];
			M[i][1] += CurEntry.Weight * SourcePt[i] * TargetPt[1];
			M[i][2] += CurEntry.Weight * SourcePt[i] * TargetPt[2];
		}
		
		//
		// accumulate scale factors (if desired)
		//
		//sa += a[0]*a[0]+a[1]*a[1]+a[2]*a[2];
		//sb += b[0]*b[0]+b[1]*b[1]+b[2]*b[2];
	}

	// compute required scaling factor (if desired)
	//double scale = (double)sqrt(sb/sa);

	DenseMatrix<double> Ndata(4, 4);
	double *N[4];
	for(UINT i = 0; i < 4; i++)
	{
		N[i] = Ndata[i];
		N[i][0] = 0.0f;
		N[i][1] = 0.0f;
		N[i][2] = 0.0f;
		N[i][3] = 0.0f;
	}
	// on-diagonal elements
	N[0][0] = M[0][0]+M[1][1]+M[2][2];
	N[1][1] = M[0][0]-M[1][1]-M[2][2];
	N[2][2] = -M[0][0]+M[1][1]-M[2][2];
	N[3][3] = -M[0][0]-M[1][1]+M[2][2];
	// off-diagonal elements
	N[0][1] = N[1][0] = M[1][2]-M[2][1];
	N[0][2] = N[2][0] = M[2][0]-M[0][2];
	N[0][3] = N[3][0] = M[0][1]-M[1][0];

	N[1][2] = N[2][1] = M[0][1]+M[1][0];
	N[1][3] = N[3][1] = M[2][0]+M[0][2];
	N[2][3] = N[3][2] = M[1][2]+M[2][1];

	// -- eigen-decompose N (is symmetric) --

	double eigenvectorData[4][4];
	double *eigenvectors[4],eigenvalues[4];

	eigenvectors[0] = eigenvectorData[0];
	eigenvectors[1] = eigenvectorData[1];
	eigenvectors[2] = eigenvectorData[2];
	eigenvectors[3] = eigenvectorData[3];

    Ndata.EigenSystem(eigenvalues, eigenvectors);

	// the eigenvector with the largest eigenvalue is the quaternion we want
	// (they are sorted in decreasing order for us by Eigensystem)
	float w, x, y, z;
	w = float(eigenvectors[0][0]);
	x = float(eigenvectors[1][0]);
	y = float(eigenvectors[2][0]);
	z = float(eigenvectors[3][0]);
	
	// convert quaternion to a rotation matrix

	float ww = w*w;
	float wx = w*x;
	float wy = w*y;
	float wz = w*z;

	float xx = x*x;
	float yy = y*y;
	float zz = z*z;

	float xy = x*y;
	float xz = x*z;
	float yz = y*z;

	SourceToTargetTransform[0][0] = ww + xx - yy - zz; 
	SourceToTargetTransform[1][0] = 2.0f*(wz + xy);
	SourceToTargetTransform[2][0] = 2.0f*(-wy + xz);

	SourceToTargetTransform[0][1] = 2.0f*(-wz + xy);  
	SourceToTargetTransform[1][1] = ww - xx + yy - zz;
	SourceToTargetTransform[2][1] = 2.0f*(wx + yz);

	SourceToTargetTransform[0][2] = 2.0f*(wy + xz);
	SourceToTargetTransform[1][2] = 2.0f*(-wx + yz);
	SourceToTargetTransform[2][2] = ww - xx - yy + zz;

	/*if (this->Mode != VTK_LANDMARK_RIGIDBODY)
	{ // add in the scale factor (if desired)
		for(i=0;i<3;i++) 
		{
			SourceToTarget[i][0] *= scale;
			SourceToTarget[i][1] *= scale;
			SourceToTarget[i][2] *= scale;
		}
	}*/

	//
	// the translation is given by the difference in the transformed source
	// centroid and the target centroid
	//
	float sx, sy, sz;

	sx = SourceToTargetTransform[0][0] * SourceCentroid[0] +
		 SourceToTargetTransform[0][1] * SourceCentroid[1] +
		 SourceToTargetTransform[0][2] * SourceCentroid[2];
	sy = SourceToTargetTransform[1][0] * SourceCentroid[0] +
		 SourceToTargetTransform[1][1] * SourceCentroid[1] +
		 SourceToTargetTransform[1][2] * SourceCentroid[2];
	sz = SourceToTargetTransform[2][0] * SourceCentroid[0] +
		 SourceToTargetTransform[2][1] * SourceCentroid[1] +
		 SourceToTargetTransform[2][2] * SourceCentroid[2];

	SourceToTargetTransform[0][3] = TargetCentroid[0] - sx;
	SourceToTargetTransform[1][3] = TargetCentroid[1] - sy;
	SourceToTargetTransform[2][3] = TargetCentroid[2] - sz;

	// fill the bottom row of the 4x4 matrix
	SourceToTargetTransform[3][0] = 0.0f;
	SourceToTargetTransform[3][1] = 0.0f;
	SourceToTargetTransform[3][2] = 0.0f;
	SourceToTargetTransform[3][3] = 1.0f;

	SourceToTargetTransform = SourceToTargetTransform.Transpose();
}

/*void GeometryAlgorithms::ComputeRigidMatchTransform(Matrix4 &Result, const Vector<Vec3f> &SourcePoints, const Vector<Vec3f> &DestPoints)
{
    //
    // MeshLab version
    //
    DenseMatrix CrossCovariance;
    Vec3f bfix,bmov; // baricenter of src e trg
    ccm.CrossCovariance(Pmov,Pfix,bmov,bfix);
    Matrix33x cyc; // the cyclic components of the cross covariance matrix.

    cyc=ccm-ccm.transpose();

    Matrix44x QQ;
    QQ.SetZero();
    Point3x D(cyc[1][2],cyc[2][0],cyc[0][1]);

    Matrix33x RM;
    RM.SetZero();
    RM[0][0]=-ccm.Trace();
    RM[1][1]=-ccm.Trace();
    RM[2][2]=-ccm.Trace();
    RM += ccm + ccm.transpose();

    QQ[0][0] = ccm.Trace();
    QQ[0][1] = D[0]; QQ[0][2] = D[1]; QQ[0][3] = D[2];
    QQ[1][0] = D[0]; QQ[2][0] = D[1];	QQ[3][0] = D[2];

    int i,j;
    for(i=0;i<3;i++)
	    for(j=0;j<3;j++)
		    QQ[i+1][j+1]=RM[i][j];

    //  printf(" Quaternion Matrix\n");
    //	print(QQ);
    Point4d d;
    Matrix44x v;
    int nrot;
    //QQ.Jacobi(d,v,nrot);
    Jacobi(QQ,d,v,nrot);
    //	printf("Done %i iterations\n %f %f %f %f\n",nrot,d[0],d[1],d[2],d[3]);
    //	print(v);
    // Now search the maximum eigenvalue
    double maxv=0;
    int maxind=-1;
    for(i=0;i<4;i++)
	    if(maxv<fabs(d[i])) {
		    q=Quaternionx(v[0][i],v[1][i],v[2][i],v[3][i]);
		    maxind=i;
		    maxv=d[i];
	    }
    // The corresponding eigenvector define the searched rotation,
    Matrix44x Rot;
    q.ToMatrix(Rot);
    // the translation (last row) is simply the difference between the transformed src barycenter and the trg baricenter
    tr= (bfix - Rot*bmov);
    //res[3][0]=tr[0];res[3][1]=tr[1];res[3][2]=tr[2];
    Matrix44x Trn;
    Trn.SetTranslate(tr);
    	
    res=Trn*Rot;
}*/